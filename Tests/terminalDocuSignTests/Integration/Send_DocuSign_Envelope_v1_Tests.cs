using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Communication;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Testing.Integration;
using Hub.StructureMap;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using StructureMap;
using terminalDocuSignTests.Fixtures;

namespace terminalDocuSignTests
{
    [Explicit]
    public class Send_DocuSign_Envelope_v1_Tests : BaseTerminalIntegrationTest
    {

        public ICrateManager _crateManager;

        [SetUp]
        public void SetUp()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);        
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
        }

        public override string TerminalName
        {
            get { return "terminalDocuSign"; }
        }

        private async Task<ActivityDTO> ConfigureInitial()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = await HealthMonitor_FixtureData.Send_DocuSign_Envelope_v1_Example_Fr8DataDTO(this);
            var responseActionDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, requestActionDTO);

            return responseActionDTO;
        }

        private async Task<ActivityDTO> ConfigureFollowUp()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var dataDTO = await HealthMonitor_FixtureData.Send_DocuSign_Envelope_v1_Example_Fr8DataDTO(this);

            var responseActionDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO);

            var storage = _crateManager.GetStorage(responseActionDTO);

            SendDocuSignEnvelope_SelectFirstTemplate(storage);

            using (var crateStorage = _crateManager.GetUpdatableStorage(dataDTO.ActivityDTO))
            {
                crateStorage.Replace(storage);
            }

            return await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO);
        }

        private void AssertCrateTypes(ICrateStorage crateStorage)
        {
            Assert.AreEqual(3, crateStorage.Count);

            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(x => x.Label == "Configuration_Controls"));
            Assert.AreEqual(1, crateStorage.CratesOfType<KeyValueListCM>().Count(x => x.Label == "Available Templates"));
            
        }

        private void AssertFollowUpCrateTypes(ICrateStorage crateStorage)
        {
            Assert.AreEqual(5, crateStorage.Count);

            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(x => x.Label == "Configuration_Controls"));
            Assert.AreEqual(1, crateStorage.CratesOfType<KeyValueListCM>().Count(x => x.Label == "Available Templates"));
            Assert.AreEqual(1, crateStorage.CratesOfType<KeyValueListCM>().Count(x => x.Label == "DocuSignTemplateUserDefinedFields"));
            Assert.AreEqual(1, crateStorage.CratesOfType<KeyValueListCM>().Count(x => x.Label == "DocuSignTemplateStandardFields"));
        }
        
        private void AssertControls(StandardConfigurationControlsCM controls)
        {
            Assert.AreEqual(2, controls.Controls.Count);

            // Assert that first control is a DropDownList 
            // with Label == "target_docusign_template"
            // and event: onChange => requestConfig.
            Assert.IsTrue(controls.Controls[0] is DropDownList);
            Assert.AreEqual("target_docusign_template", controls.Controls[0].Name);
            Assert.AreEqual(1, controls.Controls[0].Events.Count);
            Assert.AreEqual("onChange", controls.Controls[0].Events[0].Name);
            Assert.AreEqual("requestConfig", controls.Controls[0].Events[0].Handler);

            // Assert that second control is a TextSource 
            // with Label == "Recipient"
            // and event: null
            Assert.IsTrue(controls.Controls[1] is TextSource);
            Assert.AreEqual("Recipient", controls.Controls[1].Name);
            Assert.IsNull(controls.Controls[1].Events);   
        }

        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test]
        public async Task Send_DocuSign_Envelope_Initial_Configuration_Check_Crate_Structure()
        {
            var responseActionDTO = await ConfigureInitial();

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
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException),
            ExpectedMessage = @"{""status"":""terminal_error"",""message"":""One or more errors occurred.""}"
        )]
        public async Task Send_DocuSign_Envelope_Initial_Configuration_NoAuth()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var dataDTO = await HealthMonitor_FixtureData.Send_DocuSign_Envelope_v1_Example_Fr8DataDTO(this);
            dataDTO.ActivityDTO.AuthToken = null;

            await HttpPostAsync<Fr8DataDTO, JToken>(
                configureUrl,
                dataDTO
            );
        }

        /// <summary>
        /// Validate correct crate-storage structure in follow-up configuration response.
        /// </summary>
        [Test]
        public async Task Send_DocuSign_Envelope_FollowUp_Configuration_Check_Crate_Structure()
        {
            var responseFollowUpActionDTO = await ConfigureFollowUp();

            Assert.NotNull(responseFollowUpActionDTO);
            Assert.NotNull(responseFollowUpActionDTO.CrateStorage);
            Assert.NotNull(responseFollowUpActionDTO.CrateStorage.Crates);

            var crateStorage = Crate.FromDto(responseFollowUpActionDTO.CrateStorage);
            AssertFollowUpCrateTypes(crateStorage);
            AssertControls(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());
        }

        /// <summary>
        /// Wait for HTTP-500 exception when Auth-Token is not passed to followup configuration.
        /// </summary>
        [Test]
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException),
            ExpectedMessage = @"{""status"":""terminal_error"",""message"":""One or more errors occurred.""}"
        )]
        public async Task Send_DocuSign_Envelope_FollowUp_Configuration_NoAuth()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var dataDTO = await HealthMonitor_FixtureData.Send_DocuSign_Envelope_v1_Example_Fr8DataDTO(this);

            var responseActionDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO);

            var storage = _crateManager.GetStorage(responseActionDTO);

            SendDocuSignEnvelope_SelectFirstTemplate(storage);

            using (var crateStorage = _crateManager.GetUpdatableStorage(dataDTO.ActivityDTO))
            {
                crateStorage.Replace(storage);
            }

            dataDTO.ActivityDTO.AuthToken = null;

            await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO);
        }

        /// <summary>
        /// Test run-time for action from Monitor_DocuSign_FollowUp_Configuration_TemplateValue.
        /// </summary>
        [Test]
        public async Task Send_DocuSign_Envelope_Run_With_Specific_Recipient()
        {
            var runUrl = GetTerminalRunUrl();
            var configureUrl = GetTerminalConfigureUrl();
            var dataDTO = await HealthMonitor_FixtureData.Send_DocuSign_Envelope_v1_Example_Fr8DataDTO(this);
            var responseActionDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO);
            var storage = _crateManager.GetStorage(responseActionDTO);

            SendDocuSignEnvelope_SetSpecificRecipient(storage);

            using (var updatableStorage = _crateManager.GetUpdatableStorage(dataDTO.ActivityDTO))
            {
                updatableStorage.Replace(storage);
            }
            
            var responsePayloadDTO = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);
            var crateStorage = Crate.GetStorage(responsePayloadDTO);
            Assert.AreEqual(0, crateStorage.Count());
        }

        /// <summary>
        /// Wait for HTTP-500 exception when Auth-Token is not passed to run.
        /// </summary>
        [Test]
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException),
            ExpectedMessage = @"{""status"":""terminal_error"",""message"":""No auth token provided.""}"
        )]
        public async Task Send_DocuSign_Envelope_Run_NoAuth()
        {
            var runUrl = GetTerminalRunUrl();
            var configureUrl = GetTerminalConfigureUrl();
            var dataDTO = await HealthMonitor_FixtureData.Send_DocuSign_Envelope_v1_Example_Fr8DataDTO(this);
            var responseActionDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO);
            var storage = _crateManager.GetStorage(responseActionDTO);

            SendDocuSignEnvelope_SetSpecificRecipient(storage);

            using (var crateStorage = _crateManager.GetUpdatableStorage(dataDTO.ActivityDTO))
            {
                crateStorage.Replace(storage);
            }
            dataDTO.ActivityDTO.AuthToken = null;
            await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);
        }


        private void SendDocuSignEnvelope_SelectFirstTemplate(ICrateStorage curCrateStorage)
        {
            // Fetch Available Template crate and parse StandardDesignTimeFieldsMS.
            var availableTemplatesCrateDTO = curCrateStorage.CratesOfType<KeyValueListCM>().Single(x => x.Label == "Available Templates");

            var fieldsMS = availableTemplatesCrateDTO.Content;

            // Fetch Configuration Controls crate and parse StandardConfigurationControlsMS

            var configurationControlsCrateDTO = curCrateStorage.CratesOfType<StandardConfigurationControlsCM>().Single(x => x.Label == "Configuration_Controls");

            var controlsMS = configurationControlsCrateDTO.Content;

            // Modify value of Selected_DocuSign_Template field and push it back to crate,
            // exact same way we do on front-end.
            var docuSignTemplateControlDTO = controlsMS.Controls.Single(x => x.Name == "target_docusign_template");
            docuSignTemplateControlDTO.Value = fieldsMS.Values.First().Value;
        }

        private void SendDocuSignEnvelope_SetSpecificRecipient(ICrateStorage curCrateStorage)
        {
            var controls = curCrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            var recipient = controls.Controls.Single(c => c.Name == "Recipient") as TextSource;
            recipient.ValueSource = "specific";
            recipient.Value = "test@test.com";
        }

        [Test]
        public async Task Send_DocuSign_Envelope_Activate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalActivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = await HealthMonitor_FixtureData.Send_DocuSign_Envelope_v1_Example_Fr8DataDTO(this);

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
        public async Task Send_DocuSign_Envelope_Deactivate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalDeactivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = await HealthMonitor_FixtureData.Send_DocuSign_Envelope_v1_Example_Fr8DataDTO(this);

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
