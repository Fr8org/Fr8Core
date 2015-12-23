
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Restful;
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
    public class Write_To_Log_v1Tests : BaseHealthMonitorTest
    {
        public override string TerminalName
        {
            get { return "terminalPapertrail"; }
        }

        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test, Category("Integration.terminalPapertrail")]
        public async void Write_To_Log_Initial_Configuration_Check_Crate_Structure()
        {
            //Arrange
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Write_To_Log_v1_InitialConfiguration_ActionDTO();

            //Act
            var responseActionDTO =
                await HttpPostAsync<ActionDTO, ActionDTO>(
                    configureUrl,
                    requestActionDTO
                );

            //Assert
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            Assert.NotNull(responseActionDTO.CrateStorage.Crates);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            AssertCrateTypes(crateStorage);
            AssertControls(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());
        }

        /// <summary>
        /// Validate correct crate-storage structure in followup configuration response.
        /// </summary>
        [Test, Category("Integration.terminalPapertrail")]
        public async void Write_To_Log_FollowUp_Configuration_Check_Crate_Structure()
        {
            //Arrange
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Write_To_Log_v1_InitialConfiguration_ActionDTO();

            //Act
            //Call first time for the initial configuration
            var responseActionDTO =
                await HttpPostAsync<ActionDTO, ActionDTO>(
                    configureUrl,
                    requestActionDTO
                );

            //Call second time for the follow up configuration
            responseActionDTO =
                await HttpPostAsync<ActionDTO, ActionDTO>(
                    configureUrl,
                    requestActionDTO
                );

            //Assert
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            Assert.NotNull(responseActionDTO.CrateStorage.Crates);

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
        public async void Write_To_Log_Run_WithUpstreamActionLog_ValidTargetUrl_ShouldLogMessage()
        {
            //Arrange
            var runUrl = GetTerminalRunUrl();

            //prepare action DTO with valid target URL
            var actionDTO = await GetActionDTO_LogToPapertrailIntegrationTest();

            //add the log message in upstream action
            AddPayloadCrate(actionDTO,
                new StandardLoggingCM
                {
                    Item =
                        new List<LogItemDTO>
                        {
                            new LogItemDTO {Data = "Integration Test Log Message on " + DateTime.Now, IsLogged = false}
                        }
                });

            //Act
            var responsePayloadDTO =
                await HttpPostAsync<ActionDTO, PayloadDTO>(runUrl, actionDTO);

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
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException),
            ExpectedMessage = @"{""status"":""terminal_error"",""message"":""Papertrail URL and PORT are not in the correct format. The given URL is InvalidUrl""}"
            )]
        public async void Write_To_Log_Run_WithInvalidPapertrailUrl_ShouldThrowException()
        {
            //Arrange
            var runUrl = GetTerminalRunUrl();

            //prepare the action DTO with valid target URL
            var actionDTO = await GetActionDTO_LogToPapertrailIntegrationTest();

            //make the target URL as invalid
            using (var updater = Crate.UpdateStorage(actionDTO))
            {
                var controls = updater.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();

                var targetUrlTextBox = (TextBox)controls.Controls[0];
                targetUrlTextBox.Value = "InvalidUrl";
            }

            //add the Log Message in upstream action
            AddPayloadCrate(actionDTO,
                new StandardLoggingCM
                {
                    Item =
                        new List<LogItemDTO>
                        {
                            new LogItemDTO {Data = "Integration Test Log Message on " + DateTime.Now, IsLogged = false}
                        }
                });

            //Act
            var responsePayloadDTO = await HttpPostAsync<ActionDTO, PayloadDTO>(runUrl, actionDTO);
        }

        /// <summary>
        /// The Write_To_Log action with
        /// 1) No upstream Log Message
        /// 2) Valid Papertrail target URL
        /// Should throw expcetion
        /// </summary>
        [Test]
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException),
            ExpectedMessage = @"{""status"":""terminal_error"",""message"":""Sequence contains no elements""}"
            )]
        public async void Write_To_Log_Run_WithoutLogMessageInUpstreamAction_ShouldThrowException()
        {
            //Arrange
            var runUrl = GetTerminalRunUrl();

            //prepare action DTO with valid target URL
            var actionDTO = await GetActionDTO_LogToPapertrailIntegrationTest();

            //Act
            var responsePayloadDTO = await HttpPostAsync<ActionDTO, PayloadDTO>(runUrl, actionDTO);
        }

        private void AssertCrateTypes(CrateStorage crateStorage)
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
            Assert.AreEqual(1, controls.Controls[0].Events.Count, "Event subscription is missing.");
            Assert.AreEqual("onChange", controls.Controls[0].Events[0].Name, "onChange event is not subscribed");
            Assert.AreEqual("requestConfig", controls.Controls[0].Events[0].Handler, "requestConfig is not configured when onChange event.");
        }

        private async Task<ActionDTO> GetActionDTO_LogToPapertrailIntegrationTest()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Write_To_Log_v1_InitialConfiguration_ActionDTO();

            var responseActionDTO =
                await HttpPostAsync<ActionDTO, ActionDTO>(
                    configureUrl,
                    requestActionDTO
                );

            using (var updater = Crate.UpdateStorage(responseActionDTO))
            {
                var controls = updater.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();

                var targetUrlTextBox = (TextBox) controls.Controls[0];
                targetUrlTextBox.Value = "logs3.papertrailapp.com:22529";
            }

            return responseActionDTO;
        }

        [Test, Category("Integration.terminalPapertrail")]
        public async void Write_To_Log_Activate_Returns_ActionDTO()
        {
            //Arrange
            var configureUrl = GetTerminalActivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = HealthMonitor_FixtureData.Write_To_Log_v1_InitialConfiguration_ActionDTO();

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

        [Test, Category("Integration.terminalPapertrail")]
        public async void Write_To_Log_Deactivate_Returns_ActionDTO()
        {
            //Arrange
            var configureUrl = GetTerminalDeactivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = HealthMonitor_FixtureData.Write_To_Log_v1_InitialConfiguration_ActionDTO();

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
