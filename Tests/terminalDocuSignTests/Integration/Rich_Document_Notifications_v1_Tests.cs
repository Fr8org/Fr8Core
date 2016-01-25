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
using UtilitiesTesting.Fixtures;

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
            Assert.AreEqual(5, crateStorage.Count);
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count());
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardDesignTimeFieldsCM>().Count(x => x.Label == "AvailableTemplates"));
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardDesignTimeFieldsCM>().Count(x => x.Label == "AvailableEvents"));
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardDesignTimeFieldsCM>().Count(x => x.Label == "AvailableHandlers"));
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardDesignTimeFieldsCM>().Count(x => x.Label == "AvailableRecipientEvents"));
            
        }

        private void AssertControls(StandardConfigurationControlsCM controls)
        {
            Assert.AreEqual(3, controls.Controls.Count());
            Assert.AreEqual(2, controls.Controls.Count(x => x.Type == "RadioButtonGroup"));
            //Assert.AreEqual(1, controls.Controls.Count(x => x.Name == "SpecificEvent"));
            Assert.AreEqual(1, controls.Controls.Count(x => x.Name == "NotificationHandler"));
        }

        private void AddHubActivityTemplate(ActivityDTO activityDTO)
        {

            var terminal = new TerminalDTO()
            {
                Name = "terminalDocuSign",
                TerminalStatus = TerminalStatus.Active,
                Version = "1",
                Endpoint = GetTerminalUrl(),
                AuthenticationType = AuthenticationType.Internal
            };
            var terminalCoreDO = FixtureData.TestTerminal_Core_DTO();

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

            var setDelayActionTemplate = new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "SetDelay",
                Label = "Delay Action Processing",
                Category = ActivityCategory.Processors,
                Terminal = terminalCoreDO,
                NeedsAuthentication = false,
                MinPaneWidth = 330
            };

            var testIncomingDataTemplate = new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "TestIncomingData",
                Label = "TestIncomingData",
                Category = ActivityCategory.Processors,
                Terminal = terminalCoreDO,
                NeedsAuthentication = false
            };

            AddActivityTemplate(
               activityDTO,
              testIncomingDataTemplate
            );

            AddActivityTemplate(
               activityDTO,
              setDelayActionTemplate
            );

            var queryMTDatabaseActionTemplate = new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "QueryMTDatabase",
                Label = "Query MT Database",
                Category = ActivityCategory.Processors,
                Terminal = terminalCoreDO,
                NeedsAuthentication = false,
                MinPaneWidth = 330
            };

            AddActivityTemplate(
               activityDTO,
              queryMTDatabaseActionTemplate
            );

            AddActivityTemplate(
               activityDTO,
              docusignEventActionTemplate
            );

            AddActivityTemplate(
                activityDTO,
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
                await HttpPostAsync<ActivityDTO, ActivityDTO>(
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

        private async Task<ActivityDTO> GetActionDTO_WithEventsAndDelayValue()
        {
            var configureUrl = GetTerminalConfigureUrl();
            var requestActionDTO = HealthMonitor_FixtureData.Rich_Document_Notifications_v1_InitialConfiguration_ActionDTO();
            AddHubActivityTemplate(requestActionDTO);

            var responseActionDTO =
                await HttpPostAsync<ActivityDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                );

            using (var updater = Crate.UpdateStorage(responseActionDTO))
            {
                var controls = updater.CrateStorage
                    .CrateContentsOfType<StandardConfigurationControlsCM>()
                    .Single();

                //var templateDdl = (DropDownList)controls.Controls[1];

                var radioGroup = (RadioButtonGroup)controls.Controls[0];
                radioGroup.Radios[0].Selected = true;

                var availableEventCM = updater.CrateStorage
                    .CrateContentsOfType<StandardDesignTimeFieldsCM>(x => x.Label == "AvailableEvents")
                    .Single();

                var availableHandlers = updater.CrateStorage
                    .CrateContentsOfType<StandardDesignTimeFieldsCM>(x => x.Label == "AvailableHandlers")
                    .Single();

                Assert.IsTrue(availableEventCM.Fields.Count > 0);

                //templateDdl.Value = availableEventCM.Fields[0].Value;
                var howToBeNotifiedDdl = (DropDownList)controls.FindByName("NotificationHandler");
                howToBeNotifiedDdl.Value = availableHandlers.Fields[0].Value;

                var whenToBeNotifiedRadioGrp = (RadioButtonGroup)controls.FindByName("WhenToBeNotified");
                whenToBeNotifiedRadioGrp.Radios[0].Selected = false;
                whenToBeNotifiedRadioGrp.Radios[1].Selected = true;

                var durationControl = (Duration)whenToBeNotifiedRadioGrp.Radios[1].Controls.First(c => c.Name == "TimePeriod");
                durationControl.Days = 0;
                durationControl.Hours = 0;
                durationControl.Minutes = 2;
            }

            return responseActionDTO;
        }

        private async Task<ActivityDTO> GetActionDTO_WithEventsValue()
        {
            var configureUrl = GetTerminalConfigureUrl();
            var requestActionDTO = HealthMonitor_FixtureData.Rich_Document_Notifications_v1_InitialConfiguration_ActionDTO();
            AddHubActivityTemplate(requestActionDTO);

            var responseActionDTO =
                await HttpPostAsync<ActivityDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                );

            using (var updater = Crate.UpdateStorage(responseActionDTO))
            {
                var controls = updater.CrateStorage
                    .CrateContentsOfType<StandardConfigurationControlsCM>()
                    .Single();

                //var templateDdl = (DropDownList)controls.Controls[1];

                var radioGroup = (RadioButtonGroup)controls.Controls[0];
                radioGroup.Radios[0].Selected = true;

                var availableEventCM = updater.CrateStorage
                    .CrateContentsOfType<StandardDesignTimeFieldsCM>(x => x.Label == "AvailableEvents")
                    .Single();

                var availableHandlers = updater.CrateStorage
                    .CrateContentsOfType<StandardDesignTimeFieldsCM>(x => x.Label == "AvailableHandlers")
                    .Single();

                Assert.IsTrue(availableEventCM.Fields.Count > 0);

                //templateDdl.Value = availableEventCM.Fields[0].Value;
                var howToBeNotifiedDdl = (DropDownList)controls.FindByName("NotificationHandler");
                howToBeNotifiedDdl.Value = availableHandlers.Fields[0].Value;

            }

            return responseActionDTO;
        }
        /*
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
        */
        /*
        // check for child actions.
        [Test]
        public async void Rich_Document_Notifications_FollowUp_Configuration_Check_ChildAction_WithoutDelay()
        {
            var configureUrl = GetTerminalConfigureUrl();
            var actionDTO = await GetActionDTO_WithEventsValue();
            actionDTO.AuthToken = HealthMonitor_FixtureData.DocuSign_AuthToken();
            AddHubActivityTemplate(actionDTO);
            var responseActionDTO =
              await HttpPostAsync<ActionDTO, ActionDTO>(
                  configureUrl,
                  actionDTO
              );

             Assert.NotNull(responseActionDTO);
             Assert.NotNull(responseActionDTO.CrateStorage);
             Assert.NotNull(responseActionDTO.CrateStorage.Crates);
             Assert.AreEqual(2, responseActionDTO.ChildrenActions.Length);
             Assert.AreEqual(1, responseActionDTO.ChildrenActions.Count(x => x.Label == "Monitor DocuSign"));
        }
        */
        //This test causes timeout exception on build server. disabled for now
        /*
        [Test]
        public async void Rich_Document_Notifications_FollowUp_Configuration_Check_ChildAction_WithDelay()
        {
            var configureUrl = GetTerminalConfigureUrl();
            var actionDTO = await GetActionDTO_WithEventsAndDelayValue();
            actionDTO.AuthToken = HealthMonitor_FixtureData.DocuSign_AuthToken();
            AddHubActivityTemplate(actionDTO);
            var responseActionDTO =
              await HttpPostAsync<ActionDTO, ActionDTO>(
                  configureUrl,
                  actionDTO
              );

            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            Assert.NotNull(responseActionDTO.CrateStorage.Crates);
            Assert.AreEqual(5, responseActionDTO.ChildrenActions.Length);
            Assert.AreEqual(1, responseActionDTO.ChildrenActions.Count(x => x.Label == "Monitor DocuSign"));
            Assert.AreEqual(1, responseActionDTO.ChildrenActions.Count(x => x.Label == "Query MT Database"));
            Assert.AreEqual(1, responseActionDTO.ChildrenActions.Count(x => x.Label == "Set Delay"));
            Assert.AreEqual(1, responseActionDTO.ChildrenActions.Count(x => x.Label == "Filter Using Run Time"));
            
        }
        */
        [Test]
        public async void Rich_Document_Notifications_Activate_Returns_ActionDTO()
        {
            //Arrange
            var configureUrl = GetTerminalActivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = HealthMonitor_FixtureData.Rich_Document_Notifications_v1_InitialConfiguration_ActionDTO();

            //Act
            var responseActionDTO =
                await HttpPostAsync<ActivityDTO, ActivityDTO>(
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
                await HttpPostAsync<ActivityDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                );

            //Assert
            Assert.IsNotNull(responseActionDTO);
            Assert.IsNotNull(Crate.FromDto(responseActionDTO.CrateStorage));
        }
    }
}
