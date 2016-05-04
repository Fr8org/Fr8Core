using Data.Entities;
using Data.Interfaces;
using Hub.Interfaces;
using Hub.Managers;
using Moq;
using NUnit.Framework;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.States;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace HubTests.Managers
{
    class PlanManagerTests : BaseTest
    {
        private PlanManager planManager;

        public override void SetUp()
        {
            base.SetUp();
            SetupMonitorFr8Plan();
            planManager = new PlanManager();
        }

        [Test, Category("LogFr8Events_CreatePlan")]
        public async Task CreatePlan_LogFr8EventPlan()
        {
            // call the plan
            await planManager.CreatePlan_LogFr8InternalEvents(FixtureData.TestAdminAccount().Email);

            // assert
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Assert.IsFalse(uow.PlanRepository.GetPlanQueryUncached().Count() > 1, "Automatic plan is created in following success");
            }
        }

        // Test setup method
        private void SetupMonitorFr8Plan()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Create a test account
                var testFr8Account = FixtureData.TestAdminAccount();

                FixtureData.TestAdminAccount().Email = "system1@fr8.co";

                uow.EmailAddressRepository.GetOrCreateEmailAddress(FixtureData.TestAdminAccount().Email);
                uow.UserRepository.GetOrCreateUser(FixtureData.TestAdminAccount().Email);

                //create required activities
                var monitorFr8ActivityTemplate = FixtureData.TestActivityTemplateDO_MonitorFr8Events();
                var storeMTDataActivityTemplate = FixtureData.TestActivityTemplateDO_StoreMTData();

                uow.TerminalRepository.Add(monitorFr8ActivityTemplate.Terminal);
                uow.TerminalRepository.Add(storeMTDataActivityTemplate.Terminal);

                uow.ActivityTemplateRepository.Add(monitorFr8ActivityTemplate);
                uow.ActivityTemplateRepository.Add(storeMTDataActivityTemplate);

                uow.SaveChanges();

                // used the previous action.
                var monitorFr8Action = FixtureData.TestActivity1();
                var storeMtDataAction = FixtureData.TestActivity2();

                //setup Action Service
                Mock<IActivity> _setupMock = new Mock<IActivity>(MockBehavior.Default);

                //setup Action Service
                _setupMock.Setup(
                    a => a.CreateAndConfigure(It.IsAny<IUnitOfWork>(), It.IsAny<string>(), It.IsAny<Guid>(),
                        It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<Guid>(), false, It.IsAny<Guid?>())).Callback(() =>
                        {
                        }).Returns(Task.FromResult(monitorFr8Action as PlanNodeDO));


                _setupMock.Setup(
                   a => a.CreateAndConfigure(It.IsAny<IUnitOfWork>(), It.IsAny<string>(), It.IsAny<Guid>(),
                       It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<Guid>(), false, It.IsAny<Guid?>())).Callback(() =>
                       {

                       }).Returns(Task.FromResult(storeMtDataAction as PlanNodeDO));

                _setupMock.Setup(
                 a => a.Configure(It.IsAny<IUnitOfWork>(), It.IsAny<string>(), It.IsAny<ActivityDO>(), It.IsAny<Boolean>())).Callback(() =>
                 {
                 }).Returns(Task.FromResult(new Fr8Data.DataTransferObjects.ActivityDTO()));

                _setupMock.Setup(a => a.MapFromDTO(new Fr8Data.DataTransferObjects.ActivityDTO()));

                ObjectFactory.Container.Inject(typeof(IActivity), _setupMock.Object);
            }
        }
    }
}
