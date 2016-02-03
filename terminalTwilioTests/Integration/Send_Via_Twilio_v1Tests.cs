using System.Linq;
using Data.Constants;
using HealthMonitor.Utility;
using terminalTwilioTests.Fixture;
using Data.Control;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using NUnit.Framework;

namespace terminalTwilioTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    class Send_Via_Twilio_v1Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalTwilio"; }
        }
        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test, Category("Integration.terminalTwilio")]
        public async void Send_Via_Twilio_Initial_Configuration_Check_Crate_Structure()
        {
            //Arrange
            var configureUrl = GetTerminalConfigureUrl();
            var requestActionDTO = HealthMonitor_FixtureData.Send_Via_Twilio_v1_InitialConfiguration_Fr8DataDTO();
            //Act
            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                );
            //Assert
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            Assert.NotNull(responseActionDTO.CrateStorage.Crates);
            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            var controls = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();
            Assert.NotNull(controls.Controls[0] is TextSource);
            Assert.NotNull(controls.Controls[1] is TextBox);
            Assert.AreEqual("SMS Body", controls.Controls[1].Label);
            Assert.AreEqual("SMS_Body", controls.Controls[1].Name);
            Assert.AreEqual("Upstream Terminal-Provided Fields", controls.Controls[0].Source.Label);
            Assert.AreEqual(false, controls.Controls[0].Selected);
        }
        /// <summary>
        /// Expect null when ActionDTO with no StandardConfigurationControlsCM Crate.
        /// </summary>
        [Test, Category("Integration.terminalTwilio")]
        public async void Send_Via_Twilio_Run_With_No_SMS_Number_Provided()
        {
            //Arrange
            var runUrl = GetTerminalRunUrl();
            var dataDTO = HealthMonitor_FixtureData.Send_Via_Twilio_v1_InitialConfiguration_Fr8DataDTO();
            //Act
            //OperationalStateCM crate is required to be added,
            //as upon return the Run method takes this crate and updates the status to "Success"
            AddOperationalStateCrate(dataDTO.ActivityDTO, new OperationalStateCM());
            var payloadDTO = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(
                runUrl,
                dataDTO
                );
            //Assert
            var operationalCrate = Crate.FromDto(payloadDTO.CrateStorage).CrateContentsOfType<OperationalStateCM>().FirstOrDefault();
            Assert.AreEqual(ActivityResponse.Error, operationalCrate.CurrentActivityResponse, "Run method of the Send_Via_Twilio did not set CurentActionResponce to Error");
            Assert.AreEqual("No StandardConfigurationControlsCM crate provided", operationalCrate.CurrentActivityErrorMessage, "Run method of the Send_Via_Twilio did not set error message");

        }
        /// <summary>
        /// Test Twilio Service. Preconfigure Crates with testing number.
        /// Expect that the status of the message is not fail or undelivered.
        /// </summary>
        [Test, Category("Integration.terminalTwilio")]
        public async void Send_Via_Twilio_Run_Send_SMS_With_Correct_Number()
        {
            //Arrange
            var configureUrl = GetTerminalConfigureUrl();
            var runUrl = GetTerminalRunUrl();
            var dataDTO = HealthMonitor_FixtureData.Send_Via_Twilio_v1_InitialConfiguration_Fr8DataDTO();
            //Act
            var responseActionDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO);
            var crateManager = new CrateManager();
            using (var updater = crateManager.UpdateStorage(responseActionDTO))
            {
                var curTextSource =
                    (TextSource)
                        updater.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single().Controls[0];
                curTextSource.ValueSource = "specific";
                curTextSource.TextValue = "+15005550006";
                updater.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single().Controls[1].Value =
                    "That is the body of the message";
            }
            //OperationalStateCM crate is required to be added,
            //as upon return the Run method takes this crate and updates the status to "Success"
            AddOperationalStateCrate(responseActionDTO, new OperationalStateCM());
            dataDTO.ActivityDTO = responseActionDTO;
            var payloadDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(runUrl, dataDTO);
            //Assert
            //After Configure Test
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            Assert.NotNull(responseActionDTO.CrateStorage.Crates);
            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            var controls = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();
            Assert.NotNull(controls.Controls[0] is TextSource);
            Assert.NotNull(controls.Controls[1] is TextBox);
            Assert.AreEqual("SMS Body", controls.Controls[1].Label);
            Assert.AreEqual("SMS_Body", controls.Controls[1].Name);
            Assert.AreEqual("Upstream Terminal-Provided Fields", controls.Controls[0].Source.Label);
            Assert.AreEqual(false, controls.Controls[0].Selected);
            //After Run Test
            var payload = Crate.FromDto(payloadDTO.CrateStorage).CrateContentsOfType<StandardPayloadDataCM>().Single();
            Assert.AreEqual("Status", payload.PayloadObjects[0].PayloadObject[0].Key);
            Assert.AreNotEqual("failed", payload.PayloadObjects[0].PayloadObject[0].Value);
            Assert.AreNotEqual("undelivered", payload.PayloadObjects[0].PayloadObject[0].Value);
        }

        [Test, Category("Integration.terminalTwilio")]
        public async void Send_Via_Twilio_Activate_Returns_ActionDTO()
        {
            //Arrange
            var configureUrl = GetTerminalActivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = HealthMonitor_FixtureData.Send_Via_Twilio_v1_InitialConfiguration_Fr8DataDTO();

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

        [Test, Category("Integration.terminalTwilio")]
        public async void Send_Via_Twilio_Deactivate_Returns_ActionDTO()
        {
            //Arrange
            var configureUrl = GetTerminalDeactivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = HealthMonitor_FixtureData.Send_Via_Twilio_v1_InitialConfiguration_Fr8DataDTO();

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
