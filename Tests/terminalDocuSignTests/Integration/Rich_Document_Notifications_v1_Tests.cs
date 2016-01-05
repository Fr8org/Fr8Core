using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Data.Control;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Restful;
using terminalDocuSignTests.Fixtures;
using Data.States;
namespace terminalDocuSignTests.Integration
{
    [Explicit]
    [Category("terminalDocuSignTests.Integration")]
    public class Rich_Document_Notifications_v1_Tests : BaseHealthMonitorTest
    {
        public override string TerminalName
        {
            get { return "terminalDocuSign"; }
        }

        private void AssertCrateTypes(CrateStorage crateStorage)
        {
            Assert.AreEqual(4, crateStorage.Count);
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count());
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardDesignTimeFieldsCM>().Count(x => x.Label == "AvailableTemplates"));
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardDesignTimeFieldsCM>().Count(x => x.Label == "AvailableEvents"));
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardDesignTimeFieldsCM>().Count(x => x.Label == "AvailableHandlers"));
        }

        private void AssertControls(StandardConfigurationControlsCM controls)
        {
            Assert.AreEqual(4, controls.Controls.Count());
            Assert.AreEqual(2, controls.Controls.Count(x => x.Type == "RadioButtonGroup"));
            Assert.AreEqual(1, controls.Controls.Count(x => x.Name == "SpecificEvent"));
            Assert.AreEqual(1, controls.Controls.Count(x => x.Name == "NotificationHandler"));
        }

        private void AddHubActivityTemplate(ActionDTO actionDTO)
        {

            var terminal = new TerminalDTO()
            {
                Name = "terminalDocuSign",
                TerminalStatus = TerminalStatus.Active,
                Version = "1",
                Endpoint = GetTerminalUrl(),
                AuthenticationType = AuthenticationType.Internal
            };

            var docusignEventActionTemplate = new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "Monitor_DocuSign_Envelope_Activity",
                Label = "Monitor DocuSign Envelope Activity",
                Category = ActivityCategory.Monitors,
                Terminal = terminal,
                NeedsAuthentication = true,
                MinPaneWidth = 330
            };

            AddActivityTemplate(
               actionDTO,
              docusignEventActionTemplate
            );

            AddActivityTemplate(
                actionDTO,
                new ActivityTemplateDTO()
                {
                    Id = 9,
                    Name = "Send Email_Via_Send_Grid",
                    Label = "Send Email using SendGrid",
                    Tags = "Notifier",
                    Version = "1",
                    Terminal = terminal
                }
            );
        }

        [Test]
        public async void Rich_Document_Notification_Initial_Configuration_Check_Crate_Structure()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Rich_Document_Notifications_v1_InitialConfiguration_ActionDTO();
            AddHubActivityTemplate(requestActionDTO);
            requestActionDTO.AuthToken = HealthMonitor_FixtureData.DocuSign_AuthToken();

            var responseActionDTO =
                await HttpPostAsync<ActionDTO, ActionDTO>(
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

        private async Task<ActionDTO> GetActionDTO_WithEventsValue()
        {
            var configureUrl = GetTerminalConfigureUrl();
            var requestActionDTO = HealthMonitor_FixtureData.Rich_Document_Notifications_v1_InitialConfiguration_ActionDTO();
            AddHubActivityTemplate(requestActionDTO);

            var responseActionDTO =
                await HttpPostAsync<ActionDTO, ActionDTO>(
                    configureUrl,
                    requestActionDTO
                );

            using (var updater = Crate.UpdateStorage(responseActionDTO))
            {
                var controls = updater.CrateStorage
                    .CrateContentsOfType<StandardConfigurationControlsCM>()
                    .Single();

                var templateDdl = (DropDownList)controls.Controls[1];

                var radioGroup = (RadioButtonGroup)controls.Controls[0];
                radioGroup.Radios[0].Selected = true;

                var availableEventCM = updater.CrateStorage
                    .CrateContentsOfType<StandardDesignTimeFieldsCM>(x => x.Label == "AvailableEvents")
                    .Single();

                Assert.IsTrue(availableEventCM.Fields.Count > 0);

                templateDdl.Value = availableEventCM.Fields[0].Value;
            }

            return responseActionDTO;
        }

        // check for Follow-up configuration
        [Test]
        public async void Rich_Document_FollowUp_Configuration_Check_Crate_Structure()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var actionDTO = await GetActionDTO_WithEventsValue();

            actionDTO.AuthToken = HealthMonitor_FixtureData.DocuSign_AuthToken();

            var responseActionDTO =
                await HttpPostAsync<ActionDTO, ActionDTO>(
                    configureUrl,
                    actionDTO
                );

            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            Assert.NotNull(responseActionDTO.CrateStorage.Crates);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            AssertCrateTypes(crateStorage);
            AssertControls(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());
        }

        // check for child actions.
        [Test]
        public async void Rich_Document_Notifications_FollowUp_Configuration_Check_ChildAction()
        {
            var configureUrl = GetTerminalConfigureUrl();
            var actionDTO = await GetActionDTO_WithEventsValue();
            actionDTO.AuthToken = HealthMonitor_FixtureData.DocuSign_AuthToken();

            var responseActionDTO =
              await HttpPostAsync<ActionDTO, ActionDTO>(
                  configureUrl,
                  actionDTO
              );

             Assert.NotNull(responseActionDTO);
             Assert.NotNull(responseActionDTO.CrateStorage);
             Assert.NotNull(responseActionDTO.CrateStorage.Crates);
             Assert.AreEqual(1, responseActionDTO.ChildrenActions.Length);
             Assert.AreEqual(1, responseActionDTO.ChildrenActions.Count(x => x.Label == "Monitor DocuSign"));
        }

        [Test]
        public async void Rich_Document_Notifications_Activate_Returns_ActionDTO()
        {
            //Arrange
            var configureUrl = GetTerminalActivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = HealthMonitor_FixtureData.Rich_Document_Notifications_v1_InitialConfiguration_ActionDTO();

            //Act
            var responseActionDTO =
                await HttpPostAsync<ActionDTO, ActionDTO>(
                    configureUrl,
                    requestActionDTO
                );

            //Assert
            Assert.IsNotNull(responseActionDTO);
            Assert.IsNotNull(Crate.FromDto(responseActionDTO.CrateStorage));
        }

        [Test]
        public async void Rich_Document_Notifications_Deactivate_Returns_ActionDTO()
        {
            //Arrange
            var configureUrl = GetTerminalDeactivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = HealthMonitor_FixtureData.Rich_Document_Notifications_v1_InitialConfiguration_ActionDTO();

            //Act
            var responseActionDTO =
                await HttpPostAsync<ActionDTO, ActionDTO>(
                    configureUrl,
                    requestActionDTO
                );

            //Assert
            Assert.IsNotNull(responseActionDTO);
            Assert.IsNotNull(Crate.FromDto(responseActionDTO.CrateStorage));
        }
    }
}
