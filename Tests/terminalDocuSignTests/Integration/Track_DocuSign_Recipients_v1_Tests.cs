using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using HealthMonitor.Utility;
using terminalDocuSignTests.Fixtures;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using UtilitiesTesting.Fixtures;
using Fr8Data.Managers;

namespace terminalDocuSignTests.Integration
{
    [Explicit]
    [Category("terminalDocuSignTests.Integration")]
    public class Track_DocuSign_Recipients_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalDocuSign"; }
        }

        private void AssertCrateTypes(ICrateStorage crateStorage)
        {
            Assert.AreEqual(5, crateStorage.Count);
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count());
            Assert.AreEqual(1, crateStorage.CratesOfType<FieldDescriptionsCM>().Count(x => x.Label == "AvailableTemplates"));
            Assert.AreEqual(1, crateStorage.CratesOfType<FieldDescriptionsCM>().Count(x => x.Label == "AvailableHandlers"));
            Assert.AreEqual(1, crateStorage.CratesOfType<FieldDescriptionsCM>().Count(x => x.Label == "AvailableRecipientEvents"));
            Assert.AreEqual(1, crateStorage.CratesOfType<FieldDescriptionsCM>().Count(x => x.Label == "AvailableRunTimeDataFields"));
            


        }

        private void AssertControls(StandardConfigurationControlsCM controls)
        {
            Assert.AreEqual(5, controls.Controls.Count());

            Assert.AreEqual(1, controls.Controls.Count(x => x.Name == "Track_Which_Envelopes"));
            Assert.AreEqual(1, controls.Controls.Count(x => x.Name == "NotificationHandler"));
            Assert.AreEqual(1, controls.Controls.Count(x => x.Name == "TimePeriod"));
            Assert.AreEqual(1, controls.Controls.Count(x => x.Name == "RecipientEvent"));
            Assert.AreEqual(1, controls.Controls.Count(x => x.Name == "EventInfo"));
            
        }

        private void AddHubActivityTemplate(Fr8DataDTO dataDTO)
        {

            var terminal = new TerminalDTO()
            {
                Name = "terminalDocuSign",
                Label = "DocuSign",
                TerminalStatus = TerminalStatus.Active,
                Version = "1",
                Endpoint = TerminalUrl,
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
               dataDTO,
              testIncomingDataTemplate
            );

            AddActivityTemplate(
               dataDTO,
              setDelayActionTemplate
            );

            var queryFr8WarehouseActionTemplate = new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "QueryFr8Warehouse",
                Label = "Query Fr8 Warehouse",
                Category = ActivityCategory.Processors,
                Terminal = terminalCoreDO,
                NeedsAuthentication = false,
                MinPaneWidth = 330
            };

            AddActivityTemplate(
               dataDTO,
              queryFr8WarehouseActionTemplate
            );

            AddActivityTemplate(
               dataDTO,
              docusignEventActionTemplate
            );

            AddActivityTemplate(
                dataDTO,
                new ActivityTemplateDTO()
                {
                    Id = Guid.NewGuid(),
                    Name = "Send Email_Via_Send_Grid",
                    Label = "Send Email",
                    Tags = "Notifier",
                    Version = "1",
                    Terminal = terminal
                }
            );
        }

        [Test]
        public async Task Track_DocuSign_Recipients_Initial_Configuration_Check_Crate_Structure()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var dataDTO = await HealthMonitor_FixtureData.Track_DocuSign_Recipients_v1_InitialConfiguration_Fr8DataDTO(this);
            AddHubActivityTemplate(dataDTO);

            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    dataDTO
                );

            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            AssertCrateTypes(crateStorage);
            AssertControls(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());
        }

        private async Task<ActivityDTO> GetActivityDTO_WithEventsAndDelayValue()
        {
            var configureUrl = GetTerminalConfigureUrl();
            var dataDTO = await HealthMonitor_FixtureData.Track_DocuSign_Recipients_v1_InitialConfiguration_Fr8DataDTO(this);
            AddHubActivityTemplate(dataDTO);

            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    dataDTO
                );

            using (var crateStorage = Crate.GetUpdatableStorage(responseActionDTO))
            {
                var controls = crateStorage
                    .CrateContentsOfType<StandardConfigurationControlsCM>()
                    .Single();

                //var templateDdl = (DropDownList)controls.Controls[1];

                var radioGroup = (RadioButtonGroup)controls.Controls[0];
                radioGroup.Radios[0].Selected = true;

                var availableEventCM = crateStorage
                    .CrateContentsOfType<FieldDescriptionsCM>(x => x.Label == "AvailableEvents")
                    .Single();

                var availableHandlers = crateStorage
                    .CrateContentsOfType<FieldDescriptionsCM>(x => x.Label == "AvailableHandlers")
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

        private async Task<ActivityDTO> GetActivityDTO_WithEventsValue()
        {
            var configureUrl = GetTerminalConfigureUrl();
            var dataDTO = await HealthMonitor_FixtureData.Track_DocuSign_Recipients_v1_InitialConfiguration_Fr8DataDTO(this);
            AddHubActivityTemplate(dataDTO);

            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    dataDTO
                );

            using (var crateStorage = Crate.GetUpdatableStorage(responseActionDTO))
            {
                var controls = crateStorage
                    .CrateContentsOfType<StandardConfigurationControlsCM>()
                    .Single();

                //var templateDdl = (DropDownList)controls.Controls[1];

                var radioGroup = (RadioButtonGroup)controls.Controls[0];
                radioGroup.Radios[0].Selected = true;

                var availableEventCM = crateStorage
                    .CrateContentsOfType<FieldDescriptionsCM>(x => x.Label == "AvailableEvents")
                    .Single();

                var availableHandlers = crateStorage
                    .CrateContentsOfType<FieldDescriptionsCM>(x => x.Label == "AvailableHandlers")
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
        public async Task Rich_Document_FollowUp_Configuration_Check_Crate_Structure()
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

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            AssertCrateTypes(crateStorage);
            AssertControls(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());
        }
        */
        /*
        // check for child actions.
        [Test]
        public async Task Track_DocuSign_Recipients_FollowUp_Configuration_Check_ChildAction_WithoutDelay()
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
             Assert.AreEqual(2, responseActionDTO.ChildrenActions.Length);
             Assert.AreEqual(1, responseActionDTO.ChildrenActions.Count(x => x.Label == "Monitor DocuSign"));
        }
        */
        //This test causes timeout exception on build server. disabled for now
        /*
        [Test]
        public async Task Track_DocuSign_Recipients_FollowUp_Configuration_Check_ChildAction_WithDelay()
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
            Assert.AreEqual(5, responseActionDTO.ChildrenActions.Length);
            Assert.AreEqual(1, responseActionDTO.ChildrenActions.Count(x => x.Label == "Monitor DocuSign"));
            Assert.AreEqual(1, responseActionDTO.ChildrenActions.Count(x => x.Label == "Query MT Database"));
            Assert.AreEqual(1, responseActionDTO.ChildrenActions.Count(x => x.Label == "Set Delay"));
            Assert.AreEqual(1, responseActionDTO.ChildrenActions.Count(x => x.Label == "Filter Using Run Time"));
            
        }
        */
        [Test]
        public async Task Track_DocuSign_Recipients_Activate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalActivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = await HealthMonitor_FixtureData.Track_DocuSign_Recipients_v1_InitialConfiguration_Fr8DataDTO(this);

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
        public async Task Track_DocuSign_Recipients_Deactivate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalDeactivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = await HealthMonitor_FixtureData.Track_DocuSign_Recipients_v1_InitialConfiguration_Fr8DataDTO(this);

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
