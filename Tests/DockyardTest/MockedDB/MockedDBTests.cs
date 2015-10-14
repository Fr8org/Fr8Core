using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.States;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using RestSharp.Serializers;

namespace DockyardTest.MockedDB
{
    [TestFixture]
    [Category("MockedDB")]
    public class MockedDBTests : BaseTest
    {
        //This test is to ensure our mocking properly distinguishes between saved and local DbSets (to mimic EF behaviour)
        [Test]
        public void TestDBMocking()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curUser = FixtureData.TestUser1();
                curUser.Id = "1";
               
                uow.UserRepository.Add(curUser);

                using (var subUow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    Assert.AreEqual(0, subUow.UserRepository.GetQuery().Count());
                    Assert.AreEqual(0, subUow.UserRepository.DBSet.Local.Count());
                }

                Assert.AreEqual(0, uow.UserRepository.GetQuery().Count());
                Assert.AreEqual(1, uow.UserRepository.DBSet.Local.Count());

                uow.SaveChanges();

                using (var subUow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    Assert.AreEqual(1, subUow.UserRepository.GetQuery().Count());
                    Assert.AreEqual(0, subUow.UserRepository.DBSet.Local.Count());
                }
            }
        }

        [Test]
        public void TestDBMockingForeignKeyUpdate()
        {
            //using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            //{
            //    var user = new UserDO();
            //    user.State = UserState.Active;
            //    var curUser = new FixtureData(uow).TestBookingRequest1();
            //    curUser.Id = 1;
            //    curUser.CustomerID = user.Id;
            //    uow.UserRepository.Add(curUser);
            //    uow.UserRepository.Add(user);

            //    uow.SaveChanges();

            //    Assert.NotNull(curUser.Customer);
            //}
        }

        [Test]
        public void TestCollectionsProperlyUpdated()
        {
            ////Force a seed -- helps with debug
            //using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            //{
            //    var userDO = uow.UserRepository.GetOrCreateUser("testemail@gmail.com");
            //    uow.UserRepository.Add(userDO);

            //    ObjectFactory.GetInstance<ISecurityServices>().Login(uow, userDO);

            //    var negDO = new FixtureData(uow).TestNegotiation1();
            //    negDO.Id = 1;
            //    uow.NegotiationsRepository.Add(negDO);

            //    var attendee = new AttendeeDO();
            //    attendee.NegotiationID = 1;
            //    uow.AttendeeRepository.Add(attendee);

            //    uow.SaveChanges();

            //    Assert.AreEqual(1, negDO.Attendees.Count);
            //}

            //using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            //{
            //    var negDO = uow.NegotiationsRepository.GetQuery().First();

            //    Assert.AreEqual(1, negDO.Attendees.Count);
            //}
        }


        [Test]
        public void AssertAllTestsImplementBaseTest()
        {
            var failedTypes = new List<Type>();
                foreach (var testClass in GetType().Assembly.GetTypes().Where(t => t.GetCustomAttributes<TestFixtureAttribute>().Any()))
                {
                    if (testClass != typeof(BaseTest) && !testClass.IsSubclassOf(typeof(BaseTest)))
                        failedTypes.Add(testClass);
                }
                var exceptionMessages = new List<String>();
                foreach (var failedType in failedTypes)
                {
                    var testClassName = failedType.Name;
                    exceptionMessages.Add(testClassName + " must implement 'BaseTest'");
                }
                if (exceptionMessages.Any())
                    Assert.Fail(String.Join(Environment.NewLine, exceptionMessages));
            }

        // DO-1214
//		  [Test, Ignore("https://maginot.atlassian.net/browse/DO-1008")]
//		  public void ActivityRepository_AddActionDOandActivityListDO_Failed()
//		  {
//			  var activityTempate = new ActivityTemplateDO()
//			 {
//				 Id = 1,
//				 Version = "1"
//			 };
//
//			  ActionListDO al1 = new ActionListDO() { Id = 1, Ordering = 1, ActionListType = ActionListType.Immediate };
//			  ActionDO a1 = new ActionDO() { Id = 23, ActivityTemplate = activityTempate };
//			  al1.Activities.Add(a1);
//			  a1.ParentActivity = al1;
//
//			  ActionListDO al2 = new ActionListDO() { Id = 2, Ordering = 1, ActionListType = ActionListType.Immediate };
//			  ActionDO a2 = new ActionDO() { Id = 24, ActivityTemplate = activityTempate };
//			  al2.Activities.Add(a2);
//			  a2.ParentActivity = al2;
//			  using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
//			  {
//				  uow.ActivityRepository.Add(al1);
//				  uow.ActivityRepository.Add(al2);
//				  uow.SaveChanges();
//
//				  var allActivites = uow.ActivityRepository.GetAll().ToList();
//
//				  Assert.AreEqual(4, allActivites.Count);
//
//			  }
//		  }
        // DO-1214
//		  [Test, Ignore("https://maginot.atlassian.net/browse/DO-1008")]
//		  public void ActivityRepository_AddActionDOandActivityListDO_Failed2()
//		  {
//			  var activityTempate = new ActivityTemplateDO()
//			  {
//				  Id = 1,
//				  Version = "1"
//			  };
//
//			  ActionListDO al1 = new ActionListDO() { Id = 1, Ordering = 1, ActionListType = ActionListType.Immediate };
//			  ActionDO a1 = new ActionDO() { Id = 23, ActivityTemplate = activityTempate };
//			  al1.Activities.Add(a1);
//			  a1.ParentActivity = al1;
//
//			  ActionListDO al2 = new ActionListDO() { Id = 2, Ordering = 1, ActionListType = ActionListType.Immediate };
//			  ActionDO a2 = new ActionDO() { Id = 24, ActivityTemplate = activityTempate };
//			  al2.Activities.Add(a2);
//			  a2.ParentActivity = al2;
//			  using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
//			  {
//				  uow.ActivityRepository.Add(al1);
//				  uow.ActivityRepository.Add(a1);
//				  uow.ActivityRepository.Add(al2);
//				  uow.ActivityRepository.Add(a2);
//				  uow.SaveChanges();
//
//				  var allActivites = uow.ActivityRepository.GetAll().ToList();
//
//				  Assert.AreEqual(4, allActivites.Count);
//
//			  }
//		  }
            //[Test, ExpectedException(ExpectedMessage = "Foreign row does not exist.\nValue '0' on 'NegotiationDO.NegotiationState' pointing to '_NegotiationStateTemplate.Id'")]
            //public void TestForeignKeyEnforced()
            //{
            //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            //    {
            //        var br = new FixtureData(uow).TestBookingRequest1();
            //        var negDO = new NegotiationDO {Id = 1};
            //        negDO.NegotiationState = 0;
            //        negDO.BookingRequest = br;
            //        uow.NegotiationsRepository.Add(negDO);

            //        uow.SaveChanges();
            //    }
            //}

        }
    }

