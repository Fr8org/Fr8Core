using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Entities
{
    [TestFixture]
    internal class CustomFieldTests : BaseTest
    {
        private class TestCustomField : GenericCustomField<TrackingStatusDO, EmailDO>
        {
            public TestCustomField(IGenericRepository<EmailDO> foreignRepo)
                : base(foreignRepo.UnitOfWork.TrackingStatusRepository, foreignRepo)
            {
            }

            public IQueryable<EmailDO> GetEntitiesWithoutStatus()
            {
                return GetEntitiesWithoutCustomFields();
            }

            public IQueryable<EmailDO> GetEntitiesWhereTrackingStatus(
                Expression<Func<TrackingStatusDO, bool>> customFieldPredicate)
            {
                return GetEntitiesWithCustomField(customFieldPredicate);
            }

            public IQueryable<EmailDO> GetEntitiesWithStatus()
            {
                return GetEntitiesWithCustomField();
            }

            public TrackingStatusDO GetStatus(EmailDO entityDO)
            {
                return GetCustomField(entityDO);
            }

            public void SetStatus(EmailDO entityDO, int status)
            {
                GetOrCreateCustomField(entityDO).TrackingStatus = status;
            }

            public void DeleteStatus(EmailDO entityDO)
            {
                DeleteCustomField(entityDO);
            }
        }

        [Test]
        public void TestDeleteStatus()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var trackingStatus = new TestCustomField(uow.EmailRepository);

                var emailOne = new EmailDO {Id = 1};
                emailOne.From = fixture.TestEmailAddress1();

                uow.EmailRepository.Add(emailOne);

                uow.TrackingStatusRepository.Add(new TrackingStatusDO
                {
                    Id = emailOne.Id,
                    ForeignTableName = "EmailDO",
                    TrackingStatus = TrackingState.Unprocessed
                });
                uow.SaveChanges();

                Assert.AreEqual(1, uow.TrackingStatusRepository.GetAll().Count());

                trackingStatus.DeleteStatus(emailOne);
                uow.SaveChanges();
                Assert.AreEqual(0, uow.TrackingStatusRepository.GetAll().Count());

                trackingStatus.DeleteStatus(emailOne);
                uow.SaveChanges();
                Assert.AreEqual(0, uow.TrackingStatusRepository.GetAll().Count());
            }
        }

        [Test]
        public void TestGetStatus()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var trackingStatus = new TestCustomField(uow.EmailRepository);

                var emailOne = new EmailDO {Id = 1};
                var emailTwo = new EmailDO {Id = 2};
                emailOne.From = fixture.TestEmailAddress1();
                emailTwo.From = fixture.TestEmailAddress1();

                uow.EmailRepository.Add(emailOne);
                uow.EmailRepository.Add(emailTwo);

                uow.TrackingStatusRepository.Add(new TrackingStatusDO
                {
                    Id = emailOne.Id,
                    ForeignTableName = "EmailDO",
                    TrackingStatus = TrackingState.Unprocessed
                });
                uow.SaveChanges();

                TrackingStatusDO firstStatus = trackingStatus.GetStatus(emailOne);
                Assert.NotNull(firstStatus);
                Assert.AreEqual(TrackingState.Unprocessed, firstStatus.TrackingStatus);

                TrackingStatusDO secondStatus = trackingStatus.GetStatus(emailTwo);
                Assert.Null(secondStatus);
            }
        }

        [Test]
        public void TestSetStatus()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var trackingStatus = new TestCustomField(uow.EmailRepository);

                var emailOne = new EmailDO {Id = 1};
                emailOne.From = fixture.TestEmailAddress1();

                uow.EmailRepository.Add(emailOne);
                uow.SaveChanges();

                Assert.AreEqual(0, uow.TrackingStatusRepository.GetAll().Count());

                TrackingStatusDO status = trackingStatus.GetStatus(emailOne);
                Assert.Null(status);

                trackingStatus.SetStatus(emailOne, TrackingState.Unprocessed);
                uow.SaveChanges();

                status = trackingStatus.GetStatus(emailOne);
                Assert.NotNull(status);
                Assert.AreEqual(TrackingState.Unprocessed, status.TrackingStatus);
                Assert.AreEqual(1, uow.TrackingStatusRepository.GetAll().Count());

                trackingStatus.SetStatus(emailOne, TrackingState.Processed);
                uow.SaveChanges();

                status = trackingStatus.GetStatus(emailOne);
                Assert.NotNull(status);
                Assert.AreEqual(TrackingState.Processed, status.TrackingStatus);
                Assert.AreEqual(1, uow.TrackingStatusRepository.GetAll().Count());
            }
        }

        [Test]
        public void TestWhereTrackingStatus()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var trackingStatus = new TestCustomField(uow.EmailRepository);

                var emailOne = new EmailDO {Id = 1};
                var emailTwo = new EmailDO {Id = 2};
                emailOne.From = fixture.TestEmailAddress1();
                emailTwo.From = fixture.TestEmailAddress1();

                uow.EmailRepository.Add(emailOne);
                uow.EmailRepository.Add(emailTwo);

                uow.TrackingStatusRepository.Add(new TrackingStatusDO
                {
                    Id = emailOne.Id,
                    ForeignTableName = "EmailDO",
                    TrackingStatus = TrackingState.Unprocessed
                });
                uow.TrackingStatusRepository.Add(new TrackingStatusDO
                {
                    Id = emailTwo.Id,
                    ForeignTableName = "EmailDO",
                    TrackingStatus = TrackingState.Processed
                });
                uow.SaveChanges();

                List<EmailDO> t = trackingStatus.GetEntitiesWhereTrackingStatus(ts => ts.TrackingStatus == TrackingState.Processed).ToList();
                Assert.AreEqual(2, uow.EmailRepository.GetAll().Count());
                Assert.AreEqual(1, t.Count);
                Assert.AreEqual(emailTwo.Id, t.First().Id);
            }
        }

        [Test]
        public void TestWithStatus()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var trackingStatus = new TestCustomField(uow.EmailRepository);

                var emailOne = new EmailDO {Id = 1};
                var emailTwo = new EmailDO {Id = 2};

                emailOne.From = fixture.TestEmailAddress1();
                emailTwo.From = fixture.TestEmailAddress1();

                uow.EmailRepository.Add(emailOne);
                uow.EmailRepository.Add(emailTwo);

                uow.TrackingStatusRepository.Add(new TrackingStatusDO
                {
                    Id = emailOne.Id,
                    ForeignTableName = "EmailDO",
                    TrackingStatus = TrackingState.Unprocessed
                });
                uow.TrackingStatusRepository.Add(new TrackingStatusDO
                {
                    Id = emailTwo.Id,
                    ForeignTableName = "EmailDO",
                    TrackingStatus = TrackingState.Processed
                });
                uow.SaveChanges();

                List<EmailDO> t = trackingStatus.GetEntitiesWithStatus().ToList();
                Assert.AreEqual(2, uow.EmailRepository.GetAll().Count());
                Assert.AreEqual(2, t.Count);
                Assert.AreEqual(emailOne.Id, t.First().Id);
                Assert.AreEqual(emailTwo.Id, t.Skip(1).First().Id);
            }
        }

        [Test]
        public void TestWithoutStatus()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var trackingStatus = new TestCustomField(uow.EmailRepository);

                var emailOne = new EmailDO {Id = 1};
                var emailTwo = new EmailDO {Id = 2};
                emailOne.From = fixture.TestEmailAddress1();
                emailTwo.From = fixture.TestEmailAddress1();

                uow.EmailRepository.Add(emailOne);
                uow.EmailRepository.Add(emailTwo);

                uow.TrackingStatusRepository.Add(new TrackingStatusDO
                {
                    Id = emailOne.Id,
                    ForeignTableName = "EmailDO",
                    TrackingStatus = TrackingState.Unprocessed
                });
                uow.SaveChanges();

                List<EmailDO> t = trackingStatus.GetEntitiesWithoutStatus().ToList();
                Assert.AreEqual(2, uow.EmailRepository.GetAll().Count());
                Assert.AreEqual(1, t.Count);
                Assert.AreEqual(emailTwo.Id, t.First().Id);
            }
        }
    }
}