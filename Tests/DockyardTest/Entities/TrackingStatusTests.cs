using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Entities
{
    public class TrackingStatusTests
    {
        private TrackingStatus<EmailDO> _trackingStatus;
        private EmailRepository _emailRepo;
        private TrackingStatusRepository _trackingStatusRepository;
        private IUnitOfWork _uow;
        private FixtureData _fixture;

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies("test");
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();

            _trackingStatusRepository = new TrackingStatusRepository(_uow);
            _emailRepo = new EmailRepository(_uow);


            _trackingStatus = new TrackingStatus<EmailDO>(_trackingStatusRepository, _emailRepo);

            _fixture = new FixtureData(_uow);
        }


        [Test]
        public void TestDeleteStatus()
        {
            var emailOne = new EmailDO { EmailID = 1, From = _fixture.TestEmail1() };

            _emailRepo.Add(emailOne);

            _trackingStatusRepository.Add(new TrackingStatusDO
            {
                ForeignTableID = emailOne.EmailID,
                ForeignTableName = "EmailDO",
                Type = TrackingType.BOOKING_STATE,
                Status = TrackingStatus.UNPROCESSED
            });
            _uow.SaveChanges();

            Assert.AreEqual(1, _trackingStatusRepository.GetAll().Count());

            _trackingStatus.DeleteStatus(TrackingType.TEST_STATE, emailOne);
            _uow.SaveChanges();
            Assert.AreEqual(1, _trackingStatusRepository.GetAll().Count());

            _trackingStatus.DeleteStatus(TrackingType.BOOKING_STATE, emailOne);
            _uow.SaveChanges();
            Assert.AreEqual(0, _trackingStatusRepository.GetAll().Count());

            _trackingStatus.DeleteStatus(TrackingType.BOOKING_STATE, emailOne);
            _uow.SaveChanges();
            Assert.AreEqual(0, _trackingStatusRepository.GetAll().Count());
        }

        [Test]
        public void TestGetStatus()
        {
            var emailOne = new EmailDO { EmailID = 1, From = _fixture.TestEmail1() };
            var emailTwo = new EmailDO { EmailID = 2, From = _fixture.TestEmail1() };

            _emailRepo.Add(emailOne);
            _emailRepo.Add(emailTwo);

            _trackingStatusRepository.Add(new TrackingStatusDO
            {
                ForeignTableID = emailOne.EmailID,
                ForeignTableName = "EmailDO",
                Type = TrackingType.BOOKING_STATE,
                Status = TrackingStatus.UNPROCESSED
            });

            _trackingStatusRepository.Add(new TrackingStatusDO
            {
                ForeignTableID = emailTwo.EmailID,
                ForeignTableName = "EmailDO",
                Type = TrackingType.TEST_STATE,
                Status = TrackingStatus.UNPROCESSED
            });
            _uow.SaveChanges();

            TrackingStatusDO firstStatus = _trackingStatus.GetStatus(TrackingType.BOOKING_STATE, emailOne);
            Assert.NotNull(firstStatus);
            Assert.AreEqual(TrackingStatus.UNPROCESSED, firstStatus.Status);

            TrackingStatusDO secondStatus = _trackingStatus.GetStatus(TrackingType.BOOKING_STATE, emailTwo);
            Assert.Null(secondStatus);
        }

        [Test]
        public void TestSetStatus()
        {
            var emailOne = new EmailDO { EmailID = 1, From = _fixture.TestEmail1() };

            _emailRepo.Add(emailOne);
            _uow.SaveChanges();

            Assert.AreEqual(0, _trackingStatusRepository.GetAll().Count());

            TrackingStatusDO status = _trackingStatus.GetStatus(TrackingType.BOOKING_STATE, emailOne);
            Assert.Null(status);

            _trackingStatus.SetStatus(TrackingType.BOOKING_STATE, emailOne, TrackingStatus.UNPROCESSED);
            _uow.SaveChanges();

            status = _trackingStatus.GetStatus(TrackingType.BOOKING_STATE, emailOne);
            Assert.NotNull(status);
            Assert.AreEqual(TrackingStatus.UNPROCESSED, status.Status);
            Assert.AreEqual(1, _trackingStatusRepository.GetAll().Count());

            _trackingStatus.SetStatus(TrackingType.BOOKING_STATE, emailOne, TrackingStatus.PROCESSED);
            _uow.SaveChanges();

            status = _trackingStatus.GetStatus(TrackingType.BOOKING_STATE, emailOne);
            Assert.NotNull(status);
            Assert.AreEqual(TrackingStatus.PROCESSED, status.Status);
            Assert.AreEqual(1, _trackingStatusRepository.GetAll().Count());

            _trackingStatus.SetStatus(TrackingType.TEST_STATE, emailOne, TrackingStatus.UNPROCESSED);
            _uow.SaveChanges();

            status = _trackingStatus.GetStatus(TrackingType.BOOKING_STATE, emailOne);
            Assert.NotNull(status);
            Assert.AreEqual(TrackingStatus.PROCESSED, status.Status);
            Assert.AreEqual(2, _trackingStatusRepository.GetAll().Count());
        }

        [Test]
        public void TestWhereTrackingStatus()
        {
            var emailOne = new EmailDO { EmailID = 1, From = _fixture.TestEmail1() };
            var emailTwo = new EmailDO { EmailID = 2, From = _fixture.TestEmail1() };

            _emailRepo.Add(emailOne);
            _emailRepo.Add(emailTwo);

            _trackingStatusRepository.Add(new TrackingStatusDO
            {
                ForeignTableID = emailOne.EmailID,
                ForeignTableName = "EmailDO",
                Type = TrackingType.BOOKING_STATE,
                Status = TrackingStatus.UNPROCESSED
            });
            _trackingStatusRepository.Add(new TrackingStatusDO
            {
                ForeignTableID = emailTwo.EmailID,
                ForeignTableName = "EmailDO",
                Type = TrackingType.BOOKING_STATE,
                Status = TrackingStatus.PROCESSED
            });

            _trackingStatusRepository.Add(new TrackingStatusDO
            {
                ForeignTableID = emailTwo.EmailID,
                ForeignTableName = "EmailDO",
                Type = TrackingType.TEST_STATE,
                Status = TrackingStatus.PROCESSED
            });
            _uow.SaveChanges();

            List<EmailDO> t = _trackingStatus.GetEntitiesWhereTrackingStatus(ts => ts.Type==TrackingType.BOOKING_STATE && ts.Status == TrackingStatus.PROCESSED).ToList();
            Assert.AreEqual(2, _emailRepo.GetAll().Count());
            Assert.AreEqual(1, t.Count);
            Assert.AreEqual(emailTwo.EmailID, t.First().EmailID);
        }

        [Test]
        public void TestWithStatus()
        {
            var emailOne = new EmailDO { EmailID = 1, From = _fixture.TestEmail1() };
            var emailTwo = new EmailDO { EmailID = 2, From = _fixture.TestEmail1() };

            _emailRepo.Add(emailOne);
            _emailRepo.Add(emailTwo);

            _trackingStatusRepository.Add(new TrackingStatusDO
            {
                ForeignTableID = emailOne.EmailID,
                ForeignTableName = "EmailDO",
                Type = TrackingType.BOOKING_STATE,
                Status = TrackingStatus.UNPROCESSED
            });
            _trackingStatusRepository.Add(new TrackingStatusDO
            {
                ForeignTableID = emailTwo.EmailID,
                ForeignTableName = "EmailDO",
                Type = TrackingType.TEST_STATE,
                Status = TrackingStatus.PROCESSED
            });
            _uow.SaveChanges();

            List<EmailDO> t = _trackingStatus.GetEntitiesWithStatus(TrackingType.BOOKING_STATE).ToList();
            Assert.AreEqual(2, _emailRepo.GetAll().Count());
            Assert.AreEqual(1, t.Count);
            Assert.AreEqual(emailOne.EmailID, t.First().EmailID);
        }

        [Test]
        public void TestGetEntitiesWithoutStatus()
        {
            var emailOne = new EmailDO { EmailID = 1, From = _fixture.TestEmail1() };
            var emailTwo = new EmailDO { EmailID = 2, From = _fixture.TestEmail1() };

            _emailRepo.Add(emailOne);
            _emailRepo.Add(emailTwo);

            _trackingStatusRepository.Add(new TrackingStatusDO
            {
                ForeignTableID = emailOne.EmailID,
                ForeignTableName = "EmailDO",
                Type = TrackingType.BOOKING_STATE,
                Status = TrackingStatus.UNPROCESSED
            });

            _trackingStatusRepository.Add(new TrackingStatusDO
            {
                ForeignTableID = emailTwo.EmailID,
                ForeignTableName = "EmailDO",
                Type = TrackingType.TEST_STATE,
                Status = TrackingStatus.UNPROCESSED
            });

            _uow.SaveChanges();

            List<EmailDO> t = _trackingStatus.GetEntitiesWithoutStatus(TrackingType.BOOKING_STATE).ToList();
            Assert.AreEqual(2, _emailRepo.GetAll().Count());
            Assert.AreEqual(1, t.Count);
            Assert.AreEqual(emailTwo.EmailID, t.First().EmailID);
        }


        //[Test]
        //public void TestGetUnprocessedEntities()
        //{
        //    EmailDO emailOne = new EmailDO { EmailID = 1, From = _fixture.TestEmail1() };
        //    EmailDO emailTwo = new EmailDO { EmailID = 2, From = _fixture.TestEmail1() };
        //    EmailDO emailThree = new EmailDO { EmailID = 3, From = _fixture.TestEmail1() };

        //    _emailRepo.Add(emailOne);
        //    _emailRepo.Add(emailTwo);
        //    _emailRepo.Add(emailThree);

        //    _trackingStatusRepository.Add(new TrackingStatusDO { ForeignTableID = emailOne.EmailID, ForeignTableName = "EmailDO", Status = TrackingStatus.PROCESSED });
        //    _trackingStatusRepository.Add(new TrackingStatusDO { ForeignTableID = emailTwo.EmailID, ForeignTableName = "EmailDO", Status = TrackingStatus.UNPROCESSED });

        //    _uow.SaveChanges();

        //    List<EmailDO> unprocessed = _trackingStatus.GetUnprocessedEntities().ToList();

        //    Assert.AreEqual(2, unprocessed.Count);
        //    Assert.IsNotNull(unprocessed.First());
        //    Assert.AreEqual(TrackingStatus.UNPROCESSED, _trackingStatus.GetStatus(unprocessed.First()).Status);

        //    Assert.Null(_trackingStatus.GetStatus(unprocessed.Skip(1).First()));
        //}
    }
}
