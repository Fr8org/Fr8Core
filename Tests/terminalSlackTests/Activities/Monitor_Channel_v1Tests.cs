using System.Linq;
using System.Threading.Tasks;
using Data.Constants;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Restful;
using terminalSlackTests.Fixtures;
using terminalSlack.Actions;
using TerminalBase.BaseClasses;

namespace terminalSlackTests.Integration
{
    //https://maginot.atlassian.net/wiki/display/DDW/Monitor_Channel_v1+test+case

    [Explicit]
    [Category("terminalSlack.Integration")]
    public class Monitor_Channel_v1Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalSlack"; }
        }

        [Test]
        public async Task MonitorChannel_AfterInitialConfigurationWithAuth_ActivityStorageIsExpected()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Monitor_Channel_v1_InitialConfiguration_Fr8DataDTO();

            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                );

            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            Assert.IsNotNull(crateStorage.FirstCrateOrDefault<CrateDescriptionCM>(x => x.Label == CrateSignaller.RuntimeCrateDescriptionsCrateLabel), "Activity storage doesn't contain crate with runtime crates descriptions");
            Assert.IsNotNull(crateStorage.FirstCrateOrDefault<FieldDescriptionsCM>(x => x.Label == Monitor_Channel_v1.SlackMessagePropertiesCrateLabel), "Activity storage doesn't contain crate with Slack message properties");
            Assert.IsNotNull(crateStorage.FirstCrateOrDefault<EventSubscriptionCM>(x => x.Label == Monitor_Channel_v1.EventSubscriptionsCrateLabel), "Activity storage doesn't contain crate with event subscriptions");
        }

        [Test]
        public async Task MonitorChannel_InitialConfigurationWithoutAuth_ReturnAuthenticationCrate()
        {
            var configureUrl = GetTerminalConfigureUrl();
            var requestDataDTO = HealthMonitor_FixtureData.Monitor_Channel_v1_InitialConfiguration_Fr8DataDTO();
            requestDataDTO.ActivityDTO.AuthToken = null;
            var responseActivityDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, requestDataDTO);
            var authCrate = Crate.GetStorage(responseActivityDTO).FirstCrateOrDefault<StandardAuthenticationCM>();
            Assert.IsNotNull(authCrate, "Authentication crate is not found in unauthenticated request response");
        }

        [Test]
        public async Task MonitorChannel_WhenMessageComesFromMonitoredChannel_MessageIsProcessed()
        {
            var runUrl = GetTerminalRunUrl();
            var dataDTO = await GetConfiguredActivityWithChannelSelected("general");
            AddOperationalStateCrate(dataDTO, new OperationalStateCM());
            var responsePayloadDTO = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);
            var crateStorage = Crate.GetStorage(responsePayloadDTO);
            var slackPayload = crateStorage.CrateContentsOfType<StandardPayloadDataCM>(x => x.Label == Monitor_Channel_v1.ResultPayloadCrateLabel).FirstOrDefault();
            Assert.IsNotNull(slackPayload, "Payload doesn't contain message received from tracked channel");
            Assert.AreEqual(1, slackPayload.AllValues().Where(a => a.Key == "text" && a.Value == "test").Count(), "Payload content doesn't match received message");
        }

        [Test]
        public async Task MonitorChannel_WhenMessageComesNotFromMonitoredChannel_ProcessingIsTerminated()
        {
            var runUrl = GetTerminalRunUrl();
            var dataDTO = await GetConfiguredActivityWithChannelSelected("random");
            AddOperationalStateCrate(dataDTO, new OperationalStateCM());
            var responsePayloadDTO = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);
            var storage = Crate.GetStorage(responsePayloadDTO);
            var operationalStateCM = storage.CrateContentsOfType<OperationalStateCM>().Single();
            Assert.AreEqual(ActivityResponse.RequestTerminate.ToString(), operationalStateCM.CurrentActivityResponse.Type, "No downstream activities should run if message comes from the wrong channel");
        }

        [Test]
        public async Task MonitorChannel_WhenAllChannelsAreMonitored_AnyMessageIsProcessed()
        {
            var runUrl = GetTerminalRunUrl();
            var dataDTO = await GetConfiguredActivityWithChannelSelected();
            AddOperationalStateCrate(dataDTO, new OperationalStateCM());
            var responsePayloadDTO = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);
            var crateStorage = Crate.GetStorage(responsePayloadDTO);
            var slackPayload = crateStorage.CrateContentsOfType<StandardPayloadDataCM>(x => x.Label == Monitor_Channel_v1.ResultPayloadCrateLabel).FirstOrDefault();
            Assert.IsNotNull(slackPayload, "Payload doesn't contain message received from non-specific channel");
            Assert.AreEqual(1, slackPayload.AllValues().Where(a => a.Key == "text" && a.Value == "test").Count(), "Payload content doesn't match received message");
        }

        private async Task<Fr8DataDTO> GetConfiguredActivityWithChannelSelected(string selectedChannel = null)
        {
            selectedChannel = selectedChannel ?? string.Empty;
            var configureUrl = GetTerminalConfigureUrl();
            var requestDataDTO = HealthMonitor_FixtureData.Monitor_Channel_v1_InitialConfiguration_Fr8DataDTO();
            var activityDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, requestDataDTO);
            requestDataDTO.ActivityDTO = activityDTO;
            activityDTO.AuthToken = HealthMonitor_FixtureData.Slack_AuthToken();
            AddPayloadCrate(
                requestDataDTO,
                 new EventReportCM()
                 {
                     EventPayload = new CrateStorage()
                    {
                        Data.Crates.Crate.FromContent(
                            "EventReport",
                            new StandardPayloadDataCM(HealthMonitor_FixtureData.SlackEventFields()
                            )
                        )
                    }
                 }
            );
            selectedChannel = selectedChannel.StartsWith("#") ? selectedChannel : $"#{selectedChannel}";
            activityDTO.UpdateControls<Monitor_Channel_v1.ActivityUi>(x =>
            {
                if (string.IsNullOrEmpty(selectedChannel))
                {
                    x.AllChannelsOption.Selected = true;
                    x.SpecificChannelOption.Selected = false;
                }
                else
                {
                    x.AllChannelsOption.Selected = false;
                    x.SpecificChannelOption.Selected = true;
                    var channelListItem = x.ChannelList.ListItems.FirstOrDefault(y => y.Key == selectedChannel);
                    x.ChannelList.selectedKey = channelListItem?.Key;
                    x.ChannelList.Value = channelListItem?.Value;
                }
            });
            return requestDataDTO;
        }

        [Test]
        public async Task Monitor_Channel_Activate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalActivateUrl();
            var requestActionDTO = HealthMonitor_FixtureData.Monitor_Channel_v1_InitialConfiguration_Fr8DataDTO();
            using (var storage = Crate.GetUpdatableStorage(requestActionDTO.ActivityDTO))
            {
                storage.Add(Data.Crates.Crate.FromContent("Configuration Values", new Monitor_Channel_v1.ActivityUi(), Data.States.AvailabilityType.Configuration));
            }
            //Act
            var responseActionDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, requestActionDTO);
            //Assert
            Assert.IsNotNull(responseActionDTO);
            Assert.IsNotNull(Crate.FromDto(responseActionDTO.CrateStorage));
        }

        [Test]
        public async Task Monitor_Channel_Deactivate_Returns_ActionDTO()
        {
            //Arrange
            var configureUrl = GetTerminalDeactivateUrl();
            var requestActionDTO = HealthMonitor_FixtureData.Monitor_Channel_v1_InitialConfiguration_Fr8DataDTO();
            using (var storage = Crate.GetUpdatableStorage(requestActionDTO.ActivityDTO))
            {
                storage.Add(Data.Crates.Crate.FromContent("Configuration Values", new Monitor_Channel_v1.ActivityUi(), Data.States.AvailabilityType.Configuration));
            }
            //Act
            var responseActionDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, requestActionDTO);
            //Assert
            Assert.IsNotNull(responseActionDTO);
            Assert.IsNotNull(Crate.FromDto(responseActionDTO.CrateStorage));
        }
    }


}
