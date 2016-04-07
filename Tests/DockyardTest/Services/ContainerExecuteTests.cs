using System;
using System.Collections.Generic;
using Data.Crates;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;
// This alias is used to avoid ambiguity between StructureMap.IContainer and Core.Interfaces.IContainer
using InternalInterface = Hub.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Services;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Moq;
using System.Threading.Tasks;

namespace DockyardTest.Services
{
    
    [TestFixture]
    [Category("ContainerExecute")]
    public class ContainerExecuteTests : BaseTest
    {
        private InternalInterface.IContainer _container;

        [SetUp]
        //constructor method as it is run at the test start
        public override void SetUp()
        {
            base.SetUp();

            _container = ObjectFactory.GetInstance<InternalInterface.IContainer>();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Execute_ContainerDoIsNull_ThrowsArgumentNullException()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                await _container.Run(uow, null);
            }
        }
        
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Execute_ContainerDoCurrentActivityIsNull_ThrowsArgumentNullException()
        {
            //Get ProcessDO entity from static partial class FixtureData for already prepared data
            //The CurrentActivity value is already set to null and pass it immediately to service
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                await _container.Run(uow, FixtureData.TestContainerCurrentActivityNULL());
            }
        }

        // DO-1270
        //        [Test]
        //        public async Task Execute_CurrentActivityStateIsActive_ExpectedException()
        //        {
        //            string crateStorage = GetCrateStorageAsString();
        //
        //            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //            {
        //                var processDO = FixtureData.TestProcessExecute();
        //                var currAction = FixtureData.TestAction4();
        //                currAction.CrateStorage = crateStorage;
        //                var nextAction = FixtureData.TestAction5();
        //                nextAction.CrateStorage = crateStorage;
        //                processDO.CurrentActivity = currAction;
        //                processDO.NextActivity = nextAction;
        //
        //                uow.ProcessRepository.Add(processDO);
        //                uow.ActivityRepository.Add(currAction);
        //                uow.ActivityRepository.Add(nextAction);
        //
        //                uow.SaveChanges();
        //            }
        //            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //            {
        //                var processDO = uow.ProcessRepository.GetByKey(49);
        //                var ex = Assert.Throws<Exception>(async () => await _process.Execute(uow, processDO));
        //                Assert.AreEqual("Action ID: 3 status is 4.", ex.Message);
        //            }
        //        }
        //        [Test]
        //        public async Task Execute_CurrentActivityStateIsDeactive_ExpectedException()
        //        {
        //            string crateStorage = GetCrateStorageAsString();
        //
        //            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //            {
        //                var processDO = FixtureData.TestProcessExecute();
        //                var currAction = FixtureData.TestAction4();
        //                currAction.CrateStorage = crateStorage;
        //                var nextAction = FixtureData.TestAction5();
        //                nextAction.CrateStorage = crateStorage;
        //                processDO.CurrentActivity = currAction;
        //                processDO.NextActivity = nextAction;
        //
        //                uow.ProcessRepository.Add(processDO);
        //                uow.ActivityRepository.Add(currAction);
        //                uow.ActivityRepository.Add(nextAction);
        //
        //                uow.SaveChanges();
        //            }
        //            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //            {
        //                var processDO = uow.ProcessRepository.GetByKey(49);
        //                var ex = Assert.Throws<Exception>(async () => await _process.Execute(uow, processDO));
        //                Assert.AreEqual("Action ID: 3 status is 4.", ex.Message);
        //            }
        //        }
        //        [Test]
        //        public async Task Execute_CurrentActivityStateIsError_ExpectedException()
        //        {
        //            string crateStorage = GetCrateStorageAsString();
        //
        //            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //            {
        //                var processDO = FixtureData.TestProcessExecute();
        //                var currAction = FixtureData.TestAction4();
        //                currAction.CrateStorage = crateStorage;
        //                var nextAction = FixtureData.TestAction5();
        //                nextAction.CrateStorage = crateStorage;
        //                processDO.CurrentActivity = currAction;
        //                processDO.NextActivity = nextAction;
        //
        //                uow.ProcessRepository.Add(processDO);
        //                uow.ActivityRepository.Add(currAction);
        //                uow.ActivityRepository.Add(nextAction);
        //
        //                uow.SaveChanges();
        //            }
        //            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //            {
        //                var processDO = uow.ProcessRepository.GetByKey(49);
        //                var ex = Assert.Throws<Exception>(async () => await _process.Execute(uow, processDO));
        //                Assert.AreEqual("Action ID: 3 status is 4.", ex.Message);
        //            }
        //        }
        [Test]
        public async Task Execute_OneActivity_ShouldBeOk()
        {
            string crateStorage = GetCrateStorageAsString();


            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var containerDO = FixtureData.TestContainerExecute();
             
                
                var currAction = FixtureData.TestActivity4();
                currAction.CrateStorage = crateStorage;
                var nextAction = FixtureData.TestActivity5();
                nextAction.CrateStorage = crateStorage;
                
                containerDO.CurrentActivityId = currAction.Id;
                containerDO.NextActivityId = nextAction.Id;

                uow.UserRepository.Add(FixtureData.TestDeveloperAccount());
                uow.ActivityTemplateRepository.Add(currAction.ActivityTemplate);
                uow.ActivityTemplateRepository.Add(nextAction.ActivityTemplate);


                uow.PlanRepository.Add(new PlanDO()
                {
                    Fr8Account = FixtureData.TestDeveloperAccount(),
                    Name = "name",
                    PlanState = PlanState.Active,
                    ChildNodes = { currAction, nextAction }
                });

                uow.ContainerRepository.Add(containerDO);

                uow.SaveChanges();
            }
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var containerDO = uow.ContainerRepository.GetByKey(FixtureData.TestContainer_Id_49());
                await _container.Run(uow, containerDO);

                Assert.IsNull(containerDO.CurrentActivityId);
               // Assert.IsNull(containerDO.NextActivity);
            }
        }
        [Test]
        public async Task Execute_ManyActivities_ShouldBeOk()
        {
            string crateStorage = GetCrateStorageAsString();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var containerDO = FixtureData.TestContainerExecute();
                var currActivity = FixtureData.TestActivityTreeWithActivityTemplates();
                uow.ActivityTemplateRepository.Add(FixtureData.ActivityTemplate());

                uow.PlanRepository.Add(new PlanDO()
                {
                    Fr8Account = FixtureData.TestDeveloperAccount(),
                    Name = "name",
                    PlanState = PlanState.Active,
                    ChildNodes = { currActivity }
                });

                uow.UserRepository.Add(FixtureData.TestDeveloperAccount());
                uow.ActivityTemplateRepository.Add(currActivity.ActivityTemplate);

                containerDO.CurrentActivityId = currActivity.Id;
                uow.ContainerRepository.Add(containerDO);
                
                uow.SaveChanges();
            }
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var containerDO = uow.ContainerRepository.GetByKey(FixtureData.TestContainer_Id_49());
                await _container.Run(uow, containerDO);

                Assert.IsNull(containerDO.CurrentActivityId);
               // Assert.IsNull(processDO.NextActivity);
            }
        }



        private static string GetCrateStorageAsString()
        {
            var curCratesDTO = FixtureData.TestCrateDTO1();
            
            var tmp = new ActivityDO();

            using (var crateStorage = ObjectFactory.GetInstance<ICrateManager>().GetUpdatableStorage(tmp))
            {
                crateStorage.AddRange(curCratesDTO);
            }

            return tmp.CrateStorage;

        }	

    }

}
