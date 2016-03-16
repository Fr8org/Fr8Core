using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using NUnit.Framework;
using terminalSlackTests.Fixtures;

namespace terminalSlackTests.Integration
{
    [Explicit]
    [Category("terminalSlack.Integration")]
    public class Monitor_Channel_v1_Tests: BaseTerminalIntegrationTest
    {
         private const string infoTextBlockValue =
            @"Slack doesn't currently offer a way for us to automatically request events for this channel. 
                    To enable events to be sent to Fr8, do the following: </br>
                    <ol>
                        <li>Go to https://{yourteamname}.slack.com/services/new/outgoing-webhook. </li>
                        <li>Click 'Add Outgoing WebHooks Integration'</li>
                        <li>In the Outgoing WebHook form go to 'URL(s)' field fill the following address: 
                            <strong>https://terminalslack.fr8.co/terminals/terminalslack/events</strong>
                        </li>
                    </ol>";
        public override string TerminalName
        {
            get { return "terminalSlack"; }
        }
        [Test]
        public async Task Monitor_Channel_v1_Initial_Configuration_Check_Crate_Structure()
        {
            // Act
            var responseActionDTO = await ConfigureInitial();

            // Assert
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            Assert.NotNull(responseActionDTO.CrateStorage.Crates);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            AssertCrateTypes(crateStorage);
            AssertControls(crateStorage);
        }
        private async Task<ActivityDTO> ConfigureInitial(bool isAuthToken = true)
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Monitor_Channel_v1_InitialConfiguration_Fr8DataDTO(isAuthToken);
            var responseActionDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, requestActionDTO);

            return responseActionDTO;
        }
        private void AssertCrateTypes(ICrateStorage crateStorage)
        {
            Assert.AreEqual(3, crateStorage.Count);

            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(x => x.Label == "Configuration_Controls"));
            Assert.AreEqual(1, crateStorage.CratesOfType<FieldDescriptionsCM>().Count(x => x.Label == "Available Fields"));
            Assert.AreEqual(1, crateStorage.CratesOfType<EventSubscriptionCM>().Count(x => x.Label == "Standard Event Subscriptions"));
        }

        private void AssertControls(ICrateStorage crateStorage)
        {
            var controls = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single().Controls;
            Assert.AreEqual(2, controls.Count);
            Assert.AreEqual(1, controls.Count(x=>x.Name == "Selected_Slack_Channel" && x.Type == ControlTypes.DropDownList), "There is no DropDownList control");
            Assert.AreEqual(1, controls.Count(x=>x.Name == "Info_Label" && x.Type == ControlTypes.TextBlock), "There is no information block on initial configuration");
            Assert.AreEqual(1, controls.Count(x=>x.Value == infoTextBlockValue), "Information block reflects incorrect data");

        }
    }
}
