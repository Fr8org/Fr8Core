using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using Fr8.Testing.Unit;
using Moq;
using NUnit.Framework;
using StructureMap;
using terminalStatX.Activities;
using terminalStatX.DataTransferObjects;
using terminalStatX.Interfaces;
using terminalStatXTests.Fixtures;

namespace terminalStatXTests.Activities
{
    [TestFixture, Category("terminalStatX")]
    public class Monitor_Stat_Changes_v1_Tests : BaseTest
    {
        private static readonly CrateManager CrateManager = new CrateManager();

        private static readonly AuthorizationToken AuthorizationToken = new AuthorizationToken { Token = "1" };

        public override void SetUp()
        {
            base.SetUp();
            var hubCommunicatorMock = new Mock<IHubCommunicator>();
            ObjectFactory.Container.Inject(hubCommunicatorMock);
            ObjectFactory.Container.Inject(hubCommunicatorMock.Object);
            FixtureData.ConfigureHubToReturnEmptyPayload();
            var statXIntegrationMock = new Mock<IStatXIntegration>();
            statXIntegrationMock.Setup(x => x.GetGroups(It.IsAny<StatXAuthDTO>()))
                                .Returns(Task.FromResult(new List<StatXGroupDTO> { new StatXGroupDTO() {Id = Guid.NewGuid().ToString(), Name = "Test Group"} }));
            ObjectFactory.Container.Inject(statXIntegrationMock);
            ObjectFactory.Container.Inject(statXIntegrationMock.Object);
        }

        [Test]
        public async Task Initialize_Always_LoadsGroupList()
        {
            var activity = New<Monitor_Stat_Changes_v1>();
            var activityContext = new ActivityContext
            {
                HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage()
                },
                AuthorizationToken = AuthorizationToken
            };
            await activity.Configure(activityContext);
            ObjectFactory.GetInstance<Mock<IStatXIntegration>>().Verify(x => x.GetGroups(It.IsAny<StatXAuthDTO>()), "Stats Group list was not loaded from StatX");
        }

        [Test]
        public async Task Initialize_Always_HasEventSubscriptonCrate()
        {
            var activity = New<Monitor_Stat_Changes_v1>();
            var activityContext = new ActivityContext
            {
                HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage()
                },
                AuthorizationToken = AuthorizationToken
            };
            await activity.Configure(activityContext);
            Assert.IsNotNull(activityContext.ActivityPayload.CrateStorage.FirstCrateOrDefault<EventSubscriptionCM>(), "Event subscription was not created");
        }

        [Test]
        public async Task Initialize_Always_ReportsRuntimeAvailableFields()
        {
            var activity = New<Monitor_Stat_Changes_v1>();
            var activityContext = new ActivityContext
            {
                HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage()
                },
                AuthorizationToken = AuthorizationToken
            };
            await activity.Configure(activityContext);
            var currentActivityStorage = activityContext.ActivityPayload.CrateStorage;
            var runtimeCratesDescriptionCrate = currentActivityStorage.FirstCrateOrDefault<CrateDescriptionCM>();
            Assert.IsNotNull(runtimeCratesDescriptionCrate, "Runtime crates description crate was not created");
            Assert.IsTrue(runtimeCratesDescriptionCrate.Content.CrateDescriptions[0].Fields.Count > 0, "Runtime available fields were not reported");
        }
    }
}
