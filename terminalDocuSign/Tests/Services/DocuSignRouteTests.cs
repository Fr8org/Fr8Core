using System;
using System.Linq;
using System.Threading.Tasks;
using Data.Interfaces;
using Moq;
using NUnit.Framework;
using StructureMap;
using terminalDocuSign.Services;
using Fr8.Testing.Unit;
using Fr8.Testing.Unit.Fixtures;
using Data.Entities;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.Infrastructure;
using Fr8.TerminalBase.Interfaces;
using IActivity = Hub.Interfaces.IActivity;

namespace terminalDocuSign.Tests.Services
{
    [TestFixture]
    public class DocuSignPlanTests : BaseTest
    {
        private DocuSignPlan _curDocuSignPlan;

        public override void SetUp()
        {
            base.SetUp();

            TerminalBootstrapper.ConfigureTest();
            ObjectFactory.Container.Configure(TerminalDocusignStructureMapBootstrapper.LiveConfiguration);

            SetupForAutomaticPlan();
            _curDocuSignPlan = ObjectFactory.GetInstance<DocuSignPlan>();
        }

        private IHubCommunicator CreateHubCommunicator(string userId)
        {
            var hubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>();

            //hubCommunicator.Authorize(userId);

            return hubCommunicator;
        }

        [Test, Category("DocuSignPlan_CreatePlan")]
        [Ignore] // this test is not run on CI and didn't work locally
        public async Task CreatePlan_InitialAuthenticationSuccessful_MonitorAllDocuSignEvents_PlanCreatedWithTwoActivities()
        {
            //Act
            await _curDocuSignPlan.CreatePlan_MonitorAllDocuSignEvents(CreateHubCommunicator(FixtureData.TestDeveloperAccount().Id), null);

            //Assert
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Assert.AreEqual(1, uow.PlanRepository.GetPlanQueryUncached().Count(), "Automatic plan is not created");

                var automaticPlan = uow.PlanRepository.GetPlanQueryUncached().First();

                Assert.AreEqual("MonitorAllDocuSignEvents", automaticPlan.Name, "Automatic plan name is wrong");
                Assert.AreEqual(1, automaticPlan.SubPlans.Count(), "Automatic subPlan is not created");
                Assert.AreEqual(2, automaticPlan.SubPlans.First().ChildNodes.Count, "Automatic plan does not contain required actions");
            }
        }

        [Test, Category("DocuSignPlan_CreatePlan")]
        [Ignore] // this test is not run on CI and didn't work locally
        public async Task CreatePlan_SameUserAuthentication_MonitorAllDocuSignEvents_PlanCreatedOnlyOnce()
        {
            //call for first time auth successfull
            await _curDocuSignPlan.CreatePlan_MonitorAllDocuSignEvents(CreateHubCommunicator(FixtureData.TestDeveloperAccount().Id), null);

            //Act
            //if we call second time, the plan should not be created again.
            await _curDocuSignPlan.CreatePlan_MonitorAllDocuSignEvents(CreateHubCommunicator(FixtureData.TestDeveloperAccount().Id), null);

            //Assert
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Assert.IsFalse(uow.PlanRepository.GetPlanQueryUncached().Count() > 1, "Automatic plan is created in following authentication success");
            }
        }

        private void SetupForAutomaticPlan()
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
                    a => a.CreateAndConfigure(It.IsAny<IUnitOfWork>(), It.IsAny<string>(), It.IsAny<Guid>(),
                        It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<Guid>(), false, It.IsAny<Guid?>(), It.IsAny<PlanVisibility>())).Callback(() =>
                        {
                            using (var uow1 = ObjectFactory.GetInstance<IUnitOfWork>())
                            {
                                var subPlan = uow1.PlanRepository.GetById<SubplanDO>(uow1.PlanRepository.GetNodesQueryUncached().OfType<SubplanDO>().First().Id);
                                subPlan.ChildNodes.Add(recordDocuSignAction);

                                uow1.SaveChanges();
                            }
                        }).Returns(Task.FromResult(recordDocuSignAction as PlanNodeDO));

                _actionMock.Setup(
                    a => a.CreateAndConfigure(It.IsAny<IUnitOfWork>(), It.IsAny<string>(), It.IsAny<Guid>(),
                        It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<Guid>(), false, It.IsAny<Guid?>(), It.IsAny<PlanVisibility>())).Callback(() =>
                        {
                            using (var uow1 = ObjectFactory.GetInstance<IUnitOfWork>())
                            {
                                var subPlan = uow1.PlanRepository.GetById<SubplanDO>(uow1.PlanRepository.GetNodesQueryUncached().OfType<SubplanDO>().First().Id);
                                subPlan.ChildNodes.Add(storeMtDataAction);

                                uow1.SaveChanges();
                            }
                        }).Returns(Task.FromResult(storeMtDataAction as PlanNodeDO));

                ObjectFactory.Container.Inject(typeof (IActivity), _actionMock.Object);
            }
        }
    }
}