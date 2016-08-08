using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
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
    public class Create_Stat_v1_Tests : BaseTest
    {
        private static readonly AuthorizationToken AuthorizationToken = new AuthorizationToken
        {
            Token = "{\"authToken\":\"1\", \"apiKey\":\"1\"}"
        };

        public override void SetUp()
        {
            base.SetUp();

            var hubCommunicatorMock = new Mock<IHubCommunicator>();
            ObjectFactory.Container.Inject(hubCommunicatorMock);
            ObjectFactory.Container.Inject(hubCommunicatorMock.Object);
            FixtureData.ConfigureHubToReturnEmptyPayload();
            var statXIntegrationMock = new Mock<IStatXIntegration>();
            statXIntegrationMock.Setup(x => x.GetGroups(It.IsAny<StatXAuthDTO>()))
                .Returns(
                    Task.FromResult(new List<StatXGroupDTO>
                    {
                        new StatXGroupDTO() {Id = Guid.NewGuid().ToString(), Name = "Test Group"}
                    }));

            statXIntegrationMock.Setup(x => x.GetStatsForGroup(It.IsAny<StatXAuthDTO>(), It.IsAny<string>()))
                .Returns(
                    Task.FromResult(new List<BaseStatDTO>
                    {
                        new GeneralStatWithItemsDTO() {Id = Guid.NewGuid().ToString(), Title = "Test Stat"}
                    }));
            ObjectFactory.Container.Inject(statXIntegrationMock);
            ObjectFactory.Container.Inject(statXIntegrationMock.Object);
            ObjectFactory.Container.Inject(new Mock<IStatXPolling>().Object);
        }

        [Test]
        public async Task Initialize_Always_LoadsGroupList()
        {
            var activity = New<Create_Stat_v1>();
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
            ObjectFactory.GetInstance<Mock<IStatXIntegration>>()
                .Verify(x => x.GetGroups(It.IsAny<StatXAuthDTO>()), "Stats Group list was not loaded from StatX");
        }

        [Test]
        public async Task Initialize_Always_Include_Standard_StatType()
        {
            var activity = New<Create_Stat_v1>();
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
            var standardConfiguraitonControlsCrate =
                currentActivityStorage.FirstCrateOrDefault<StandardConfigurationControlsCM>();

            var typesDropDown =
                standardConfiguraitonControlsCrate.Content.Controls.OfType<DropDownList>()
                    .First(x => x.Name == "StatTypesList");
            Assert.AreEqual(typesDropDown.ListItems.Count, 5, "Default StatTypes are missing an Item");
        }

        [Test]
        public async Task Followup_Should_Generate_Dynamic_Controls()
        {
            var activity = New<Create_Stat_v1>();
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
            var standardConfiguraitonControlsCrate =
                currentActivityStorage.FirstCrateOrDefault<StandardConfigurationControlsCM>();

            standardConfiguraitonControlsCrate.Content.Controls.OfType<DropDownList>().First(x => x.Name == "StatTypesList").Value = "NUMBER";

            await activity.Configure(activityContext);
            currentActivityStorage = activityContext.ActivityPayload.CrateStorage;
            standardConfiguraitonControlsCrate =
                currentActivityStorage.FirstCrateOrDefault<StandardConfigurationControlsCM>();

            Assert.IsNotEmpty(standardConfiguraitonControlsCrate.Content.Controls.OfType<TextSource>().ToList(), "Dynamic Text Sources for Number Stat were not generated");
        }
    }
}