using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.DataTransferObjects.Helpers;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Testing.Integration;

using terminalTwilioTests.Fixture;
using NUnit.Framework;
using System.Configuration;

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
        public async Task Send_Via_Twilio_Initial_Configuration_Check_Crate_Structure()
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
            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            var controls = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();
            Assert.NotNull(controls.Controls[0] is TextSource);
            Assert.NotNull(controls.Controls[1] is TextSource);
            Assert.AreEqual(false, controls.Controls[0].Selected);
        }
        /// <summary>
        /// Expect null when ActionDTO with no StandardConfigurationControlsCM Crate.
        /// </summary>
        [Test, Category("Integration.terminalTwilio")]
        public async Task Send_Via_Twilio_Run_With_No_SMS_Number_Provided()
        {
            //Arrange
            var runUrl = GetTerminalRunUrl();
            var dataDTO = HealthMonitor_FixtureData.Send_Via_Twilio_v1_InitialConfiguration_Fr8DataDTO();
            //Act
            //OperationalStateCM crate is required to be added,
            //as upon return the Run method takes this crate and updates the status to "Success"
            AddOperationalStateCrate(dataDTO, new OperationalStateCM());
            var payloadDTO = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(
                runUrl,
                dataDTO
                );
            //Assert
            var operationalCrate = Crate.FromDto(payloadDTO.CrateStorage).CrateContentsOfType<OperationalStateCM>().FirstOrDefault();
            ErrorDTO errorMessage;
            operationalCrate.CurrentActivityResponse.TryParseErrorDTO(out errorMessage);

            Assert.AreEqual(ActivityResponse.Error.ToString(), operationalCrate.CurrentActivityResponse.Type, "Run method of the Send_Via_Twilio did not set CurentActionResponce to Error");
        }
        /// <summary>
        /// Test Twilio Service. Preconfigure Crates with testing number.
        /// Expect that the status of the message is not fail or undelivered.
        /// </summary>
        [Test, Category("Integration.terminalTwilio")]
        public async Task Send_Via_Twilio_Run_Send_SMS_With_Correct_Number()
        {
            //Arrange
            var configureUrl = GetTerminalConfigureUrl();
            var runUrl = GetTerminalRunUrl();
            var dataDTO = HealthMonitor_FixtureData.Send_Via_Twilio_v1_InitialConfiguration_Fr8DataDTO();
            //Act
            var responseActionDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO);
            var crateManager = new CrateManager();
            using (var updatableStorage = crateManager.GetUpdatableStorage(responseActionDTO))
            {
                var curNumberTextSource =
                    (TextSource)
                        updatableStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single().Controls[0];
                curNumberTextSource.ValueSource = "specific";
                curNumberTextSource.TextValue = ConfigurationManager.AppSettings["TestPhoneNumber"];

                var curBodyTextSource =
                   (TextSource)
                       updatableStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single().Controls[1];
                curBodyTextSource.ValueSource = "specific";
                curBodyTextSource.TextValue = "That is the body of the message";
            }
            dataDTO.ActivityDTO = responseActionDTO;
            //OperationalStateCM crate is required to be added,
            //as upon return the Run method takes this crate and updates the status to "Success"
            AddOperationalStateCrate(dataDTO, new OperationalStateCM());
            
            var payloadDTO = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);
            
            //Assert
            //After Configure Test
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            var controls = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();
            Assert.NotNull(controls.Controls[0] is TextSource);
            Assert.NotNull(controls.Controls[1] is TextSource);
            Assert.AreEqual(false, controls.Controls[0].Selected);
            //After Run Test
            var payload = Crate.FromDto(payloadDTO.CrateStorage).CrateContentsOfType<StandardPayloadDataCM>().Single();
            Assert.AreEqual("Status", payload.PayloadObjects[0].PayloadObject[0].Key);
            Assert.AreNotEqual("failed", payload.PayloadObjects[0].PayloadObject[0].Value);
            Assert.AreNotEqual("undelivered", payload.PayloadObjects[0].PayloadObject[0].Value);
        }

        [Test, Category("Integration.terminalTwilio")]
        public async Task Send_Via_Twilio_Activate_Returns_ActivityDTO()
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
        public async Task Send_Via_Twilio_Deactivate_Returns_ActivityDTO()
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
