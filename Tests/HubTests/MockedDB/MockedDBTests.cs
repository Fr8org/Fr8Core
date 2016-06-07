using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;
using Fr8.Testing.Unit;
using Fr8.Testing.Unit.Fixtures;

namespace HubTests.MockedDB
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
        }
    }

