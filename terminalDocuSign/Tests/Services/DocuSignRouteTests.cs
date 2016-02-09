using System;
using System.Linq;
using System.Threading.Tasks;
using Data.Interfaces;
using Data.States;
using Hub.Interfaces;
using Moq;
using NUnit.Framework;
using StructureMap;
using terminalDocuSign.Services;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Data.Entities;

namespace terminalDocuSign.Tests.Services
{
    [TestFixture]
    public class DocuSignRouteTests : BaseTest
    {
        private DocuSignRoute _curDocuSignRoute;

        public override void SetUp()
        {
            base.SetUp();

            SetupForAutomaticRoute();

            _curDocuSignRoute = new DocuSignRoute();
        }

        [Test, Category("DocuSignRoute_CreateRoute")]
        public async Task CreateRoute_InitialAuthenticationSuccessful_MonitorAllDocuSignEvents_RouteCreatedWithTwoActions()
        {
            //Act
            await _curDocuSignRoute.CreateRoute_MonitorAllDocuSignEvents(FixtureData.TestDeveloperAccount().Id, null);

            //Assert
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Assert.AreEqual(1, uow.PlanRepository.GetPlanQueryUncached().Count(), "Automatic plan is not created");

                var automaticRoute = uow.PlanRepository.GetPlanQueryUncached().First();

                Assert.AreEqual("MonitorAllDocuSignEvents", automaticRoute.Name, "Automatic plan name is wrong");
                Assert.AreEqual(1, automaticRoute.Subroutes.Count(), "Automatic subroute is not created");
                Assert.AreEqual(2, automaticRoute.Subroutes.First().ChildNodes.Count, "Automatic plan does not contain required actions");
            }
        }

        [Test, Category("DocuSignRoute_CreateRoute")]
        public async Task CreateRoute_SameUserAuthentication_MonitorAllDocuSignEvents_RouteCreatedOnlyOnce()
        {
            //call for first time auth successfull
            await _curDocuSignRoute.CreateRoute_MonitorAllDocuSignEvents(FixtureData.TestDeveloperAccount().Id, null);

            //Act
            //if we call second time, the plan should not be created again.
            await _curDocuSignRoute.CreateRoute_MonitorAllDocuSignEvents(FixtureData.TestDeveloperAccount().Id, null);

            //Assert
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Assert.IsFalse(uow.PlanRepository.GetPlanQueryUncached().Count() > 1, "Automatic plan is created in following authentication success");
            }
        }

        private void SetupForAutomaticRoute()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Create a test account
                var testFr8Account = FixtureData.TestDeveloperAccount();
                testFr8Account.Email = "test@email.com";
                uow.UserRepository.Add(testFr8Account);

                //create required activities
                var recordDocuSignActivityTemplate = FixtureData.TestActivityTemplateDO_RecordDocuSignEvents();
                var storeMTDataActivityTemplate = FixtureData.TestActivityTemplateDO_StoreMTData();

                // TODO: fix this.
                // recordDocuSignActivityTemplate.AuthenticationType = storeMTDataActivityTemplate.AuthenticationType = AuthenticationType.None;

                uow.TerminalRepository.Add(recordDocuSignActivityTemplate.Terminal);
                uow.TerminalRepository.Add(storeMTDataActivityTemplate.Terminal);

                uow.ActivityTemplateRepository.Add(recordDocuSignActivityTemplate);
                uow.ActivityTemplateRepository.Add(storeMTDataActivityTemplate);

                uow.SaveChanges();

                //create and mock required acitons
                var recordDocuSignAction = FixtureData.TestActivity1();
                var storeMtDataAction = FixtureData.TestActivity2();

                //setup Action Service
                Mock<IActivity> _actionMock = new Mock<IActivity>(MockBehavior.Default);

                _actionMock.Setup(
                    a => a.CreateAndConfigure(It.IsAny<IUnitOfWork>(), It.IsAny<string>(), It.IsAny<int>(),
                        "Record_DocuSign_Events", It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<Guid>(), false, It.IsAny<Guid?>())).Callback(() =>
                        {
                            using (var uow1 = ObjectFactory.GetInstance<IUnitOfWork>())
                            {
                                var subroute = uow1.PlanRepository.GetById<SubrouteDO>(uow1.PlanRepository.GetNodesQueryUncached().OfType<SubrouteDO>().First().Id);
                                subroute.ChildNodes.Add(recordDocuSignAction);

                                uow1.SaveChanges();
                            }
                        }).Returns(Task.FromResult(recordDocuSignAction as RouteNodeDO));

                _actionMock.Setup(
                    a => a.CreateAndConfigure(It.IsAny<IUnitOfWork>(), It.IsAny<string>(), It.IsAny<int>(),
                        "StoreMTData", It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<Guid>(), false, It.IsAny<Guid?>())).Callback(() =>
                        {
                            using (var uow1 = ObjectFactory.GetInstance<IUnitOfWork>())
                            {
                                var subroute = uow1.PlanRepository.GetById<SubrouteDO>(uow1.PlanRepository.GetNodesQueryUncached().OfType<SubrouteDO>().First().Id);
                                subroute.ChildNodes.Add(storeMtDataAction);

                                uow1.SaveChanges();
                            }
                        }).Returns(Task.FromResult(storeMtDataAction as RouteNodeDO));

                ObjectFactory.Container.Inject(typeof (IActivity), _actionMock.Object);
            }
        }
    }
}