using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Testing.Integration;
using NUnit.Framework;
using terminalDocuSignTests.Fixtures;

namespace terminalDocuSignTests
{
    [Explicit]
    public class Send_DocuSign_Envelope_v1_Tests : BaseTerminalIntegrationTest
    {
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

            var storage = Crate.FromDto(responseActionDTO.CrateStorage);

            SendDocuSignEnvelope_SelectFirstTemplate(storage);

            using (var crateStorage = Crate.GetUpdatableStorage(dataDTO.ActivityDTO))
            {
                crateStorage.Replace(storage);
            }

            return await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO);
        }

        private void AssertCrateTypes(ICrateStorage crateStorage)
        {
            Assert.AreEqual(1, crateStorage.Count);

            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(x => x.Label == "Configuration_Controls"));
        }

        private void AssertFollowUpCrateTypes(ICrateStorage crateStorage)
        {
            Assert.AreEqual(2, crateStorage.Count);
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(x => x.Label == "Configuration_Controls"));
            Assert.AreEqual(1, crateStorage.CratesOfType<ValidationResultsCM>().Count());
        }
        
        private void AssertControls(StandardConfigurationControlsCM controls)
        {
            Assert.AreEqual(1, controls.Controls.Count);

            // Assert that first control is a DropDownList 
            // with Label == "target_docusign_template"
            // and event: onChange => requestConfig.
            Assert.IsTrue(controls.Controls[0] is DropDownList);
            Assert.AreEqual("target_docusign_template", controls.Controls[0].Name);
            Assert.AreEqual(1, controls.Controls[0].Events.Count);
            Assert.AreEqual("onChange", controls.Controls[0].Events[0].Name);
            Assert.AreEqual("requestConfig", controls.Controls[0].Events[0].Handler);
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
        public async Task Send_DocuSign_Envelope_Initial_Configuration_NoAuth()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var dataDTO = await HealthMonitor_FixtureData.Send_DocuSign_Envelope_v1_Example_Fr8DataDTO(this);
            dataDTO.ActivityDTO.AuthToken = null;

            var response = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                configureUrl,
                dataDTO
            );
            Assert.NotNull(response);
            Assert.NotNull(response.CrateStorage);
            Assert.NotNull(response.CrateStorage.Crates);
            Assert.True(response.CrateStorage.Crates.Any(x => x.ManifestType == "Standard Authentication"));
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
        //[ExpectedException(
        //    ExpectedException = typeof(RestfulServiceException),
        //    ExpectedMessage = @"{""status"":""terminal_error"",""message"":""One or more errors occurred.""}"
        //)]
        public async Task Send_DocuSign_Envelope_FollowUp_Configuration_NoAuth()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var dataDTO = await HealthMonitor_FixtureData.Send_DocuSign_Envelope_v1_Example_Fr8DataDTO(this);

            var responseActionDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO);

            var storage = Crate.GetStorage(responseActionDTO);

            SendDocuSignEnvelope_SelectFirstTemplate(storage);

            using (var crateStorage = Crate.GetUpdatableStorage(dataDTO.ActivityDTO))
            {
                crateStorage.Replace(storage);
            }

            dataDTO.ActivityDTO.AuthToken = null;

            var response = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO);
            
            Assert.NotNull(response);
            Assert.NotNull(response.CrateStorage);
            Assert.NotNull(response.CrateStorage.Crates);
            Assert.True(response.CrateStorage.Crates.Any(x => x.ManifestType == "Standard Authentication"));
        }
        /// <summary>
        /// Wait for HTTP-500 exception when Auth-Token is not passed to run.
        /// </summary>
        [Test]
        //[ExpectedException(
        //    ExpectedException = typeof(RestfulServiceException),
        //    ExpectedMessage = @"{""status"":""terminal_error"",""message"":""No auth token provided.""}"
        //)]
        public async Task Send_DocuSign_Envelope_Run_NoAuth()
        {
            var runUrl = GetTerminalRunUrl();
            var configureUrl = GetTerminalConfigureUrl();
            var dataDTO = await HealthMonitor_FixtureData.Send_DocuSign_Envelope_v1_Example_Fr8DataDTO(this);
            var responseActionDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO);
            var storage = Crate.GetStorage(responseActionDTO);

           // SendDocuSignEnvelope_SetSpecificRecipient(storage);

            using (var crateStorage = Crate.GetUpdatableStorage(dataDTO.ActivityDTO))
            {
                crateStorage.Replace(storage);
            }
            dataDTO.ActivityDTO.AuthToken = null;
            AddOperationalStateCrate(dataDTO,new OperationalStateCM());
            var payload = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);
            CheckIfPayloadHasNeedsAuthenticationError(payload);
        }


        private void SendDocuSignEnvelope_SelectFirstTemplate(ICrateStorage curCrateStorage)
        {
            // Fetch Available Template crate and parse StandardDesignTimeFieldsMS.
            // Fetch Configuration Controls crate and parse StandardConfigurationControlsMS

            var configurationControlsCrateDTO = curCrateStorage.CratesOfType<StandardConfigurationControlsCM>().Single(x => x.Label == "Configuration_Controls");

            var controlsMS = configurationControlsCrateDTO.Content;

            // Modify value of Selected_DocuSign_Template field and push it back to crate,
            // exact same way we do on front-end.
            var docuSignTemplateControlDTO = (DropDownList)controlsMS.Controls.Single(x => x.Name == "target_docusign_template");
            docuSignTemplateControlDTO.Value = docuSignTemplateControlDTO.ListItems.First().Value;
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
