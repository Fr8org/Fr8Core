using System;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using NUnit.Framework;
using Fr8.Testing.Integration;
using terminalDocuSignTests.Fixtures;
using terminalDocuSign.Infrastructure;

namespace terminalDocuSignTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    public class Monitor_DocuSign_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalDocuSign"; }
        }

        private void AssertCrateTypes(ICrateStorage crateStorage, bool expectValidationErrors = false)
        {
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(), "Missing configuration controls");
            Assert.AreEqual(1, crateStorage.CratesOfType<EventSubscriptionCM>().Count(x => x.Label == "Standard Event Subscriptions"), "Missing Standard Event Subscriptions");

            if (expectValidationErrors)
            {
                Assert.AreEqual(1, crateStorage.CratesOfType<ValidationResultsCM>().Count(x=>x.Content.HasErrors), "Missing validation errors");
            }

            Assert.AreEqual(expectValidationErrors ? 3 : 2, crateStorage.Count, "Unexpected crates present");
        }

        private void AssertControls(StandardConfigurationControlsCM controls)
        {
            Assert.AreEqual(5, controls.Controls.Count);

            // Assert that first control is a RadioButtonGroup 
            // with Label == "TemplateRecipientPicker"
            // and event: onChange => requestConfig.
            Assert.IsTrue(controls.Controls[4] is RadioButtonGroup);
            Assert.AreEqual("TemplateRecipientPicker", controls.Controls[4].Name);
            Assert.AreEqual(1, controls.Controls[4].Events.Count);
            Assert.AreEqual("onChange", controls.Controls[4].Events[0].Name);
            Assert.AreEqual("requestConfig", controls.Controls[4].Events[0].Handler);

            // Assert that 2nd-5th controls are a CheckBoxes
            // with corresponding labels and event: onChange => requestConfig.
            var checkBoxLabels = new[] {
                "EnvelopeSent",
                "EnvelopeReceived",
                "RecipientSigned"
            };

            for (var i = 0; i < checkBoxLabels.Length; ++i)
            {
                var ci = i + 1;

                Assert.IsTrue(controls.Controls[ci] is CheckBox);
                Assert.AreEqual(checkBoxLabels[i], controls.Controls[ci].Name);
                Assert.AreEqual(1, controls.Controls[ci].Events.Count);
                Assert.AreEqual("onChange", controls.Controls[ci].Events[0].Name);
                Assert.AreEqual("requestConfig", controls.Controls[ci].Events[0].Handler);
            }

            // Assert that radio group contains two radios labeled "recipient" and "template".
            var radioButtonGroup = (RadioButtonGroup)controls.Controls[4];
            Assert.AreEqual(2, radioButtonGroup.Radios.Count);
            Assert.AreEqual("recipient", radioButtonGroup.Radios[0].Name);
            Assert.AreEqual("template", radioButtonGroup.Radios[1].Name);

            // Assert that "recipient" radio contains single TextBox control
            // labeled "RecipientValue" with event "onChange" => "requestConfig".
            Assert.AreEqual(1, radioButtonGroup.Radios[0].Controls.Count);
            Assert.IsTrue(radioButtonGroup.Radios[0].Controls[0] is TextBox);
            Assert.AreEqual("RecipientValue", radioButtonGroup.Radios[0].Controls[0].Name);
            Assert.AreEqual(1, radioButtonGroup.Radios[0].Controls[0].Events.Count);
            Assert.AreEqual("onChange", radioButtonGroup.Radios[0].Controls[0].Events[0].Name);
            Assert.AreEqual("requestConfig", radioButtonGroup.Radios[0].Controls[0].Events[0].Handler);

            // Assert that "recipient" radio contains single DropDownList control
            // labeled "UpstreamCrate", with event "onChange" => "requestConfig",
            // with source labeled "Available Templates".
            Assert.AreEqual(1, radioButtonGroup.Radios[1].Controls.Count);
            Assert.IsTrue(radioButtonGroup.Radios[1].Controls[0] is DropDownList);
            Assert.AreEqual("UpstreamCrate", radioButtonGroup.Radios[1].Controls[0].Name);
            Assert.AreEqual(null, radioButtonGroup.Radios[1].Controls[0].Source);
            Assert.AreEqual(1, radioButtonGroup.Radios[1].Controls[0].Events.Count);
            Assert.AreEqual("onChange", radioButtonGroup.Radios[1].Controls[0].Events[0].Name);
            Assert.AreEqual("requestConfig", radioButtonGroup.Radios[1].Controls[0].Events[0].Handler);
        }

        private async Task<ActivityDTO> GetActivityDTO_WithRecipientValue()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = await HealthMonitor_FixtureData.Monitor_DocuSign_v1_InitialConfiguration_Fr8DataDTO(this);

            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                );

            responseActionDTO.AuthToken = requestActionDTO.ActivityDTO.AuthToken;

            using (var crateStorage = Crate.GetUpdatableStorage(responseActionDTO))
            {
                var controls = crateStorage
                    .CrateContentsOfType<StandardConfigurationControlsCM>()
                    .Single();
                //Part of FR-2474: mark first checkbox as selected so activity will pass validation
                var checkBox = controls.Controls.OfType<CheckBox>().First();
                checkBox.Selected = true;

                var radioGroup = (RadioButtonGroup)controls.Controls[4];
                radioGroup.Radios[0].Selected = true;
                radioGroup.Radios[1].Selected = false;

                var recipientTextBox = (TextBox)radioGroup.Radios[0].Controls[0];
                recipientTextBox.Value = "hal9000@discovery.com";
            }

            return responseActionDTO;
        }

        private const string TemplateName = "Fr8 Fromentum Registration Form";

        private async Task<Tuple<ActivityDTO, string>> GetActivityDTO_WithTemplateValue()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = await HealthMonitor_FixtureData.Monitor_DocuSign_v1_InitialConfiguration_Fr8DataDTO(this);

            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                );

            responseActionDTO.AuthToken = requestActionDTO.ActivityDTO.AuthToken;

            string selectedTemplate = null;
            using (var crateStorage = Crate.GetUpdatableStorage(responseActionDTO))
            {
                var controls = crateStorage
                    .CrateContentsOfType<StandardConfigurationControlsCM>()
                    .Single();
                //Part of FR-2474: mark first checkbox as selected so activity will pass validation
                var checkBox = controls.Controls.OfType<CheckBox>().First();
                checkBox.Selected = true;

                var radioGroup = (RadioButtonGroup)controls.Controls[4];
                radioGroup.Radios[0].Selected = false;
                radioGroup.Radios[1].Selected = true;

                var templateDdl = (DropDownList)radioGroup.Radios[1].Controls[0];

                Assert.IsTrue(templateDdl.ListItems.Count > 0);
                var selectedItem = templateDdl.ListItems.FirstOrDefault(x => x.Key == TemplateName);
                Assert.IsNotNull(selectedItem, $"Template with name '{TemplateName}' doesn't exist");
                templateDdl.Value = selectedItem.Value;
                selectedTemplate = templateDdl.selectedKey = selectedItem.Key;
            }

            return new Tuple<ActivityDTO, string>(responseActionDTO, selectedTemplate);
        }

        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test]
        public async Task Monitor_DocuSign_Initial_Configuration_Check_Crate_Structure()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = await HealthMonitor_FixtureData.Monitor_DocuSign_v1_InitialConfiguration_Fr8DataDTO(this);

            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                );

            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            AssertCrateTypes(crateStorage);
            AssertControls(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());
        }

        /// <summary>
        /// Wait for HTTP-500 exception when Auth-Token is not passed to initial configuration.
        /// </summary>
        [Test]
        public async Task Monitor_DocuSign_Initial_Configuration_NoAuth()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestDataDTO = await HealthMonitor_FixtureData.Monitor_DocuSign_v1_InitialConfiguration_Fr8DataDTO(this);
            requestDataDTO.ActivityDTO.AuthToken = null;

            var response = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                configureUrl,
                requestDataDTO
            );

            Assert.NotNull(response);
            Assert.NotNull(response.CrateStorage);
            Assert.NotNull(response.CrateStorage.Crates);
            Assert.True(response.CrateStorage.Crates.Any(x => x.ManifestType == "Standard Authentication"));
        }
        
        /// <summary>
        /// Set Selected property to True of "recipient" control
        /// and set "RecipientValue" control's value to some email. 
        /// Trigger FollowUp configuration method. 
        /// Assert that result contains crate from step design-time crate labeled "DocuSign Event Fields", 
        /// that contains single field with key = "TemplateId" and empty value.
        /// </summary>
        [Test]
        public async Task Monitor_DocuSign_FollowUp_Configuration_RecipientValue()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var activityDTO = await GetActivityDTO_WithRecipientValue();
            var dataDTO = new Fr8DataDTO { ActivityDTO = activityDTO };
            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    dataDTO
                );

            var crateStorage = Crate.GetStorage(responseActionDTO);
          
            var availableFields = crateStorage
                .CrateContentsOfType<CrateDescriptionCM>(x => x.Label == "Runtime Available Crates")
                .Single();

            Assert.AreEqual(1, availableFields.CrateDescriptions.Count, "Unexpected number of available crates");
            
            Assert.AreEqual(7, availableFields.CrateDescriptions[0].Fields.Count, "Unexpected number of available fields");
        }

        /// <summary>
        /// Set Selected property to True of "template" control
        /// and set "UpstreamCrate" control's value to first value 
        /// from "Available Templates" crate. 
        /// Assert that result contains design-time crate labeled "DocuSign Event Fields",
        /// that contains single field with key = "TemplateId", 
        /// the value of that field should be equal to what was set to "UpstreamCrate" drop-down-list.
        /// </summary>
        [Test]
        public async Task Monitor_DocuSign_FollowUp_Configuration_TemplateValue()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var activityDTO = await GetActivityDTO_WithTemplateValue();
            var dataDTO = new Fr8DataDTO { ActivityDTO = activityDTO.Item1 };
            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    dataDTO
                );

            var crateStorage = Crate.GetStorage(responseActionDTO);
            var availableFields = crateStorage
                .CrateContentsOfType<CrateDescriptionCM>(x => x.Label == "Runtime Available Crates")
                .Single();

            Assert.AreEqual(1, availableFields.CrateDescriptions.Count, "Unexpected number of available crates");
            Assert.AreEqual(16, availableFields.CrateDescriptions[0].Fields.Count, "Unexpected number of available fields");
        }

        /// <summary>
        /// Wait for HTTP-500 exception when Auth-Token is not passed to initial configuration.
        /// </summary>
        [Test]
        // [ExpectedException(
        //     ExpectedException = typeof(RestfulServiceException),
        //     ExpectedMessage = @"{""status"":""terminal_error"",""message"":""No AuthToken provided.""}",
        //     MatchType = MessageMatch.Contains
        // )]
        public async Task Monitor_DocuSign_FollowUp_Configuration_NoAuth()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestDataDTO = await HealthMonitor_FixtureData.Monitor_DocuSign_v1_InitialConfiguration_Fr8DataDTO(this);

            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestDataDTO
                );

            requestDataDTO.ActivityDTO = responseActionDTO;

            var response = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                configureUrl,
                requestDataDTO
            );

            Assert.NotNull(response);
            Assert.NotNull(response.CrateStorage);
            Assert.NotNull(response.CrateStorage.Crates);
            Assert.True(response.CrateStorage.Crates.Any(x => x.ManifestType == "Standard Authentication"));
        }

        /// <summary>
        /// Test run-time for action from Monitor_DocuSign_FollowUp_Configuration_RecipientValue.
        /// </summary>
        [Test]
        public async Task Monitor_DocuSign_Run_RecipientValue()
        {
            var envelopeId = Guid.NewGuid().ToString();

            var runUrl = GetTerminalRunUrl();

            var activityDTO = await GetActivityDTO_WithRecipientValue();

            var dataDTO = new Fr8DataDTO { ActivityDTO = activityDTO };

            var payload = HealthMonitor_FixtureData.GetEnvelopePayload();

            AddPayloadCrate(
                dataDTO,
                new EventReportCM()
                {
                    EventPayload = payload,
                    EventNames = string.Join(",", DocuSignEventNames.GetAllEventNames())
                }
            );

            AddOperationalStateCrate(dataDTO, new OperationalStateCM());

            var responsePayloadDTO = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);

            var crateStorage = Crate.GetStorage(responsePayloadDTO);
            Assert.AreEqual(1, crateStorage.CrateContentsOfType<StandardPayloadDataCM>(x => x.Label == "DocuSign Envelope Fields").Count());

            var docuSignPayload = crateStorage.CrateContentsOfType<StandardPayloadDataCM>(x => x.Label == "DocuSign Envelope Fields").Single();
            Assert.AreEqual(1, docuSignPayload.AllValues().Count(x => x.Key == "CurrentRecipientEmail"));
            Assert.IsTrue(docuSignPayload.AllValues().Any(x => x.Key == "CurrentRecipientEmail" && x.Value == "hal9000@discovery.com"));
        }

        /// <summary>
        /// Test run-time for action from Monitor_DocuSign_FollowUp_Configuration_TemplateValue.
        /// </summary>
        [Test]
        public async Task Monitor_DocuSign_Run_TemplateValue()
        {
            var envelopeId = Guid.NewGuid().ToString();

            var configureUrl = GetTerminalConfigureUrl();
            var runUrl = GetTerminalRunUrl();

            var activityDTO = await GetActivityDTO_WithTemplateValue();
            var authToken = await HealthMonitor_FixtureData.DocuSign_AuthToken(this);
            activityDTO.Item1.AuthToken = authToken;

            var dataDTO = new Fr8DataDTO { ActivityDTO = activityDTO.Item1 };
            var preparedActionDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO);
            dataDTO.ActivityDTO = preparedActionDTO;
            AddOperationalStateCrate(dataDTO, new OperationalStateCM());

            AddPayloadCrate(
                dataDTO,
                new EventReportCM
                {
                    EventPayload = HealthMonitor_FixtureData.GetEnvelopePayload()
                }
            );

            preparedActionDTO.AuthToken = authToken;

            var responsePayloadDTO = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);

            var crateStorage = Crate.GetStorage(responsePayloadDTO);
            var docuSignPayload = crateStorage.CrateContentsOfType<StandardPayloadDataCM>(x => x.Label == "DocuSign Envelope Fields").SingleOrDefault();
            Assert.IsNotNull(docuSignPayload, "Crate with DocuSign envelope fields was not found in payload");
            Assert.AreEqual(16, docuSignPayload.AllValues().Count(), "DocuSign envelope fields count doesn't match expected value");
        }

        /// <summary>
        /// Test run-time without Auth-Token.
        /// </summary>
        [Test]
        public async Task Monitor_DocuSign_Run_NoAuth()
        {
            var runUrl = GetTerminalRunUrl();

            var requestDataDTO = await HealthMonitor_FixtureData.Monitor_DocuSign_v1_InitialConfiguration_Fr8DataDTO(this);
            requestDataDTO.ActivityDTO.AuthToken = null;
            AddOperationalStateCrate(requestDataDTO, new OperationalStateCM());
            var payload = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, requestDataDTO);
            CheckIfPayloadHasNeedsAuthenticationError(payload);
        }

        [Test]
        public async Task Monitor_DocuSign_Activate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalActivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestDataDTO = await HealthMonitor_FixtureData.Mail_Merge_Into_DocuSign_v1_InitialConfiguration_Fr8DataDTO(this);
            using (var crateStorage = Crate.GetUpdatableStorage(requestDataDTO.ActivityDTO))
            {
                crateStorage.Add("Configuration_Controls", new StandardConfigurationControlsCM());
            }


            //Act
            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestDataDTO
                );

            //Assert
            Assert.IsNotNull(responseActionDTO);
            Assert.IsNotNull(Crate.FromDto(responseActionDTO.CrateStorage));
        }

        [Test]
        public async Task Monitor_DocuSign_Deactivate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalDeactivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestDataDTO = await HealthMonitor_FixtureData.Monitor_DocuSign_v1_InitialConfiguration_Fr8DataDTO(this);
            using (var crateStorage = Crate.GetUpdatableStorage(requestDataDTO.ActivityDTO))
            {
                crateStorage.Add("Configuration_Controls", new StandardConfigurationControlsCM());
            }
            //Act
            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestDataDTO
                );

            //Assert
            Assert.IsNotNull(responseActionDTO);
            Assert.IsNotNull(Crate.FromDto(responseActionDTO.CrateStorage));
        }

        public async Task<string> GetOAuthToken()
        {
            return null;
        }
    }
}
