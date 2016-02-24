using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Data.Constants;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Data.Control;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.DataTransferObjects.Helpers;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Restful;
using terminalSlackTests.Fixtures;
using UtilitiesTesting;
using Newtonsoft.Json;
using StructureMap;
using Hub.StructureMap;

namespace terminalSlackTests.Integration
{
    //https://maginot.atlassian.net/wiki/display/DDW/Monitor_Channel_v1+test+case

    [Explicit]
    public class Monitor_Channel_v1Test : BaseTerminalIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalSlack"; }
        }

        [Test]
        public async Task Monitor_Channel_Initial_Configuration_Check_Crate_Structure()
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
            Assert.NotNull(responseActionDTO.CrateStorage.Crates);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            AssertCrateTypes(crateStorage);
            AssertControls(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());
        }

        private void AssertControls(StandardConfigurationControlsCM controls)
        {
            Assert.AreEqual(2, controls.Controls.Count);

            //DDLB test
            Assert.IsTrue(controls.Controls[0] is DropDownList);
            Assert.AreEqual("Selected_Slack_Channel", controls.Controls[0].Name);
            //@AlexAvrutin: Commented this since the 'Select Channel' list does not require requestConfig event. 
            //Assert.AreEqual(1, controls.Controls[0].Events.Count);
            //Assert.AreEqual("onChange", controls.Controls[0].Events[0].Name);
            //Assert.AreEqual("requestConfig", controls.Controls[0].Events[0].Handler);

            Assert.IsTrue(controls.Controls[1] is TextBlock);
            Assert.AreEqual("Info_Label", controls.Controls[1].Name);
        }

        private void AssertCrateTypes(ICrateStorage crateStorage)
        {
            Assert.AreEqual(4, crateStorage.Count);

            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count());
            Assert.AreEqual(1, crateStorage.CratesOfType<FieldDescriptionsCM>().Count(x => x.Label == "Available Fields"));
            Assert.AreEqual(1, crateStorage.CratesOfType<FieldDescriptionsCM>().Count(x => x.Label == "Available Channels"));
            Assert.AreEqual(1, crateStorage.CratesOfType<EventSubscriptionCM>().Count(x => x.Label == "Standard Event Subscriptions"));
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(RestfulServiceException))]
        public async Task Monitor_Channel_Initial_Configuration_NoAuth()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestDataDTO = HealthMonitor_FixtureData.Monitor_Channel_v1_InitialConfiguration_Fr8DataDTO();
            requestDataDTO.ActivityDTO.AuthToken = null;

            await HttpPostAsync<Fr8DataDTO, Newtonsoft.Json.Linq.JToken>(
                configureUrl,
                requestDataDTO
            );
        }

        [Test]
        public async Task Monitor_Channel_Run_RightChannel_Test()
        {
            var runUrl = GetTerminalRunUrl();

            var dataDTO = await GetConfiguredActivityWithDDLBSelected("general");
            AddOperationalStateCrate(dataDTO, new OperationalStateCM());
            
            var responsePayloadDTO = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);

            var crateStorage = Crate.GetStorage(responsePayloadDTO);

            Assert.AreEqual(1, crateStorage.CrateContentsOfType<StandardPayloadDataCM>(x => x.Label == "Slack Payload Data").Count());
            var slackPayload = crateStorage.CrateContentsOfType<StandardPayloadDataCM>(x => x.Label == "Slack Payload Data").Single();

            Assert.AreEqual(1, slackPayload.AllValues().Where(a => a.Key == "text" && a.Value == "test").Count());
        }

        [Test]
        //[ExpectedException(ExpectedException = typeof(RestfulServiceException),
        //    ExpectedMessage = "{\"status\":\"terminal_error\",\"message\":\"Unexpected channel-id.\"}")]
        public async Task Monitor_Channel_Run_WrongChannel_Test()
        {
            var runUrl = GetTerminalRunUrl();
            var dataDTO = await GetConfiguredActivityWithDDLBSelected("random");
            AddOperationalStateCrate(dataDTO, new OperationalStateCM());
            
            var responsePayloadDTO =
               await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);

            var storage = Crate.GetStorage(responsePayloadDTO);
            var operationalStateCM = storage.CrateContentsOfType<OperationalStateCM>().Single();

            ErrorDTO errorMessage;
            operationalStateCM.CurrentActivityResponse.TryParseErrorDTO(out errorMessage);

            Assert.AreEqual(ActivityResponse.Error.ToString(), operationalStateCM.CurrentActivityResponse.Type);
            Assert.AreEqual("Unexpected channel-id.", errorMessage.Message);
        }

        private async Task<Fr8DataDTO> GetConfiguredActivityWithDDLBSelected(string selectedChannel)
        { 
            var configureUrl = GetTerminalConfigureUrl();
            var requestDataDTO = HealthMonitor_FixtureData.Monitor_Channel_v1_InitialConfiguration_Fr8DataDTO();
            var activityDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestDataDTO
                );
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
            using (var crateStorage = Crate.GetUpdatableStorage(activityDTO))
            {
                var controls = crateStorage
                    .CrateContentsOfType<StandardConfigurationControlsCM>()
                    .Single();

                terminalSlack.Services.SlackIntegration slackIntegraion = new terminalSlack.Services.SlackIntegration();
                var channels = await slackIntegraion.GetChannelList(HealthMonitor_FixtureData.Slack_AuthToken().Token);

                var ddlb = (DropDownList)controls.Controls[0];
                var channel = channels.FirstOrDefault(a => a.Key == selectedChannel);
                if (channel != null)
                    ddlb.Value = channel.Value;
            }

            return requestDataDTO;
        }

        [Test]
        public async Task Monitor_Channel_Activate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalActivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = HealthMonitor_FixtureData.Monitor_Channel_v1_InitialConfiguration_Fr8DataDTO();

            //Act
            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                );

            //Assert
            Assert.IsNotNull(responseActionDTO);
            Assert.IsNotNull(Crate.FromDto(responseActionDTO.CrateStorage));
        }

        [Test]
        public async Task Monitor_Channel_Deactivate_Returns_ActionDTO()
        {
            //Arrange
            var configureUrl = GetTerminalDeactivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = HealthMonitor_FixtureData.Monitor_Channel_v1_InitialConfiguration_Fr8DataDTO();

            //Act
            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                );

            //Assert
            Assert.IsNotNull(responseActionDTO);
            Assert.IsNotNull(Crate.FromDto(responseActionDTO.CrateStorage));
        }
    }


}
