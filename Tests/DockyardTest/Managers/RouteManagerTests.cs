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
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Managers
{
    class RouteManagerTests : BaseTest
    {
        private RouteManager routeManager;

        public override void SetUp()
        {
            base.SetUp();
            SetupMonitorFr8Route();
            routeManager = new RouteManager();
        }

        [Test, Category("LogFr8Events_CreateRoute")]
        public async Task CreateRoute_LogFr8EventRoute()
        {
            // call the plan
            await routeManager.CreateRoute_LogFr8InternalEvents(FixtureData.TestAdminAccount().Email);

            // assert
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Assert.IsFalse(uow.PlanRepository.GetAll().Count() > 1, "Automatic plan is created in following success");
            }
        }

        // Test setup method
        private void SetupMonitorFr8Route()
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
                    a => a.CreateAndConfigure(It.IsAny<IUnitOfWork>(), It.IsAny<string>(), It.IsAny<int>(),
                        "Monitor_Fr8_Events", It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<Guid>(), false, It.IsAny<Guid?>())).Callback(() =>
                        {
                            using (var uow1 = ObjectFactory.GetInstance<IUnitOfWork>())
                            {
                                uow1.ActivityRepository.Add(monitorFr8Action);

                                var subRoute = uow1.SubrouteRepository.GetQuery().Single();
                                subRoute.ChildNodes.Add(monitorFr8Action);

                                uow1.SaveChanges();
                            }
                        }).Returns(Task.FromResult(monitorFr8Action as RouteNodeDO));

                _setupMock.Setup(
                    a => a.CreateAndConfigure(It.IsAny<IUnitOfWork>(), It.IsAny<string>(), It.IsAny<int>(),
                        "StoreMTData", It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<Guid>(), false, It.IsAny<Guid?>())).Callback(() =>
                        {
                            using (var uow1 = ObjectFactory.GetInstance<IUnitOfWork>())
                            {
                                uow1.ActivityRepository.Add(storeMtDataAction);

                                var subRoute = uow1.SubrouteRepository.GetQuery().Single();
                                subRoute.ChildNodes.Add(storeMtDataAction);

                                uow1.SaveChanges();
                            }
                        }).Returns(Task.FromResult(storeMtDataAction as RouteNodeDO));

                ObjectFactory.Container.Inject(typeof(IActivity), _setupMock.Object);
            }
        }
    }
}
