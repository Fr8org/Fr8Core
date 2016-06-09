using System;
using System.Collections.Generic;
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
using NUnit.Framework;
using terminalPapertrailTests.Fixtures;

namespace terminalPapertrailTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    public class Write_To_Log_v1Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalPapertrail"; }
        }

        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test, Category("Integration.terminalPapertrail")]
        public async Task Write_To_Log_Initial_Configuration_Check_Crate_Structure()
        {
            //Arrange
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Write_To_Log_v1_InitialConfiguration_Fr8DataDTO();

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
            AssertCrateTypes(crateStorage);
            AssertControls(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());
        }

        /// <summary>
        /// Validate correct crate-storage structure in followup configuration response.
        /// </summary>
        [Test, Category("Integration.terminalPapertrail")]
        public async Task Write_To_Log_FollowUp_Configuration_Check_Crate_Structure()
        {
            //Arrange
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Write_To_Log_v1_InitialConfiguration_Fr8DataDTO();

            //Act
            //Call first time for the initial configuration
            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                );

            //Call second time for the follow up configuration
            responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                );

            //Assert
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            AssertCrateTypes(crateStorage);
            AssertControls(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());
        }

        /// <summary>
        /// The Write_To_Log action with
        /// 1) Valid upstream Log Message
        /// 2) Valid Papertrail target URL
        /// Should successfully log the message
        /// </summary>
        [Test, Category("Integration.terminalPapertrail")]
        public async Task Write_To_Log_Run_WithUpstreamActivityLog_ValidTargetUrl_ShouldLogMessage()
        {
            //Arrange
            var runUrl = GetTerminalRunUrl();

            //prepare action DTO with valid target URL
            var activityDTO = await GetActivityDTO_LogToPapertrailIntegrationTest();
            var dataDTO = new Fr8DataDTO { ActivityDTO = activityDTO };
            //add the log message in upstream action
            AddPayloadCrate(dataDTO,
                new StandardLoggingCM
                {
                    Item =
                        new List<LogItemDTO>
                        {
                            new LogItemDTO {Data = "Integration Test Log Message on " + DateTime.Now, IsLogged = false}
                        }
                });

            AddOperationalStateCrate(dataDTO, new OperationalStateCM());
            
            //Act
            var responsePayloadDTO =
                await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);

            //Assert the returned paylod contains the log messages with IsLogged is set to True.
            var loggedMessageCrate = Crate.GetStorage(responsePayloadDTO).Single(x => x.Label.Equals("Log Messages"));
            Assert.IsTrue(loggedMessageCrate.IsOfType<StandardLoggingCM>(), "The returned logged message crate is not of type Standard Logging Manifest.");

            var loggedMessage = loggedMessageCrate.Get<StandardLoggingCM>();
            Assert.AreEqual(1, loggedMessage.Item.Count, "The number of logged message is not 1.");
            Assert.IsTrue(loggedMessage.Item[0].IsLogged, "The logging did not happen and the log message is not stored in Papertrail");
        }

        /// <summary>
        /// The Write_To_Log action with
        /// 1) Valid upstream Log Message
        /// 2) Invalid Papertrail target URL
        /// Should throw exception
        /// </summary>
        [Test]
        public async Task Write_To_Log_Run_WithInvalidPapertrailUrl_ShouldReturnError()
        {
            //Arrange
            var runUrl = GetTerminalRunUrl();

            //prepare the action DTO with valid target URL
            var activityDTO = await GetActivityDTO_LogToPapertrailIntegrationTest();
            var dataDTO = new Fr8DataDTO { ActivityDTO = activityDTO };
            //make the target URL as invalid
            using (var crateStorage = Crate.GetUpdatableStorage(activityDTO))
            {
                var controls = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();

                var targetUrlTextBox = (TextBox)controls.Controls[0];
                targetUrlTextBox.Value = "InvalidUrl";
            }

            //add the Log Message in upstream action
            AddPayloadCrate(dataDTO,
                new StandardLoggingCM
                {
                    Item =
                        new List<LogItemDTO>
                        {
                            new LogItemDTO {Data = "Integration Test Log Message on " + DateTime.Now, IsLogged = false}
                        }
                });

            AddOperationalStateCrate(dataDTO, new OperationalStateCM());
            
            //Act
            var payload = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);

            var storage = Crate.GetStorage(payload);
            var operationalStateCM = storage.CrateContentsOfType<OperationalStateCM>().Single();
            ErrorDTO errorMessage;
            operationalStateCM.CurrentActivityResponse.TryParseErrorDTO(out errorMessage);

            Assert.AreEqual(ActivityResponse.Error.ToString(), operationalStateCM.CurrentActivityResponse.Type);
            Assert.AreEqual("Papertrail URL and PORT are not in the correct format. The given URL is InvalidUrl", errorMessage.Message);
        }

        /// <summary>
        /// The Write_To_Log action with
        /// 1) No upstream Log Message
        /// 2) Valid Papertrail target URL
        /// Should throw expcetion
        /// </summary>
        [Test]
        public async Task Write_To_Log_Run_WithoutLogMessageInUpstreamActivity_ShouldThrowException()
        {
            //Arrange
            var runUrl = GetTerminalRunUrl();

            //prepare action DTO with valid target URL
            var activityDTO = await GetActivityDTO_LogToPapertrailIntegrationTest();
            var dataDTO = new Fr8DataDTO { ActivityDTO = activityDTO };
            AddOperationalStateCrate(dataDTO, new OperationalStateCM());
            //Act
            var responsePayloadDTO = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);

            var storage = Crate.GetStorage(responsePayloadDTO.CrateStorage);
            var opState = storage.CrateContentsOfType<OperationalStateCM>().FirstOrDefault();
            ErrorDTO error = opState.CurrentActivityResponse.TryParseErrorDTO(out error) ? error : null;
            Assert.IsTrue(error.Message.Contains("Sequence contains no elements"), $"Invalid error message: {error.Message}");
        }

        private void AssertCrateTypes(ICrateStorage crateStorage)
        {
            Assert.AreEqual(1, crateStorage.Count,
                "There should be only one crate storage in initial and follow up configuration of Write To Log action.");

            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(),
                "The target URL text box is missing in the configuration of Write To Log action.");
        }

        private void AssertControls(StandardConfigurationControlsCM controls)
        {
            Assert.AreEqual(1, controls.Controls.Count);

            // Assert that first control is a TextBox
            // with Label == "Target Papertrail URL and Port (as URL:Port)"
            // and event: onChange => requestConfig.
            Assert.IsTrue(controls.Controls[0] is TextBox, "The target Papertrail URL text box is missing.");
            Assert.AreEqual("TargetUrlTextBox", controls.Controls[0].Name, "The name of the target URL text box is wrong.");
            //@AlexAvrutin: Commented this since this textbox does not require requestConfig event. 
            //Assert.AreEqual(1, controls.Controls[0].Events.Count, "Event subscription is missing.");
            //Assert.AreEqual("onChange", controls.Controls[0].Events[0].Name, "onChange event is not subscribed");
            //Assert.AreEqual("requestConfig", controls.Controls[0].Events[0].Handler, "requestConfig is not configured when onChange event.");
        }

        private async Task<ActivityDTO> GetActivityDTO_LogToPapertrailIntegrationTest()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Write_To_Log_v1_InitialConfiguration_Fr8DataDTO();

            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                );

            using (var crateStorage = Crate.GetUpdatableStorage(responseActionDTO))
            {
                var controls = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();

                var targetUrlTextBox = (TextBox) controls.Controls[0];
                targetUrlTextBox.Value = "logs3.papertrailapp.com:22529";
            }

            return responseActionDTO;
        }

        [Test, Category("Integration.terminalPapertrail")]
        public async Task Write_To_Log_Activate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalActivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = HealthMonitor_FixtureData.Write_To_Log_v1_InitialConfiguration_Fr8DataDTO();

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

        [Test, Category("Integration.terminalPapertrail")]
        public async Task Write_To_Log_Deactivate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalDeactivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = HealthMonitor_FixtureData.Write_To_Log_v1_InitialConfiguration_Fr8DataDTO();

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
