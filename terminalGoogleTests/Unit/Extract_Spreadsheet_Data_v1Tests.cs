using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Data.Control;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using Hub.Managers.APIManagers.Transmitters.Restful;
using NUnit.Framework;
using terminalGoogleTests.Unit;

namespace terminalGoogleTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    public class Extract_Spreadsheet_Data_v1Tests: BaseHealthMonitorTest
    {
        public override string TerminalName
        {
            get { return "terminalGoogle"; }
        }

        /////////////
        /// Initial Configuration Tests Begin
        /////////////

        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test, Category("Integration.terminalGoogle")]
        public async void Extract_Spreadsheet_Data_Initial_Configuration_Check_Crate_Structure()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Extract_Spreadsheet_Data_v1_InitialConfiguration_ActionDTO();

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
        private void AssertCrateTypes(CrateStorage crateStorage)
        {
            Assert.AreEqual(1, crateStorage.Count);
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count());
        }

        private void AssertControls(StandardConfigurationControlsCM controls)
        {
            Assert.AreEqual(2, controls.Controls.Count);

            // Assert that first control is a DropDownList 
            // with Label == "Select a Google Spreadsheet"
            // and event: onChange => requestConfig.
            Assert.IsTrue(controls.Controls[0] is DropDownList);
            Assert.AreEqual("Select a Google Spreadsheet", controls.Controls[0].Label);
            Assert.AreEqual(1, controls.Controls[0].Events.Count);
            Assert.AreEqual("onChange", controls.Controls[0].Events[0].Name);
            Assert.AreEqual("requestConfig", controls.Controls[0].Events[0].Handler);
            Assert.AreEqual("This Action will try to extract a table of rows from the first worksheet in" +
                            " the selected spreadsheet. The rows should have a header row.", controls.Controls[1].Value);
        }

        /////////////
        /// Initial Configuration Tests End
        /////////////

        /////////////
        /// Followup Configuration Tests Begin
        /////////////
        [Test, Category("Integration.terminalGoogle")]
         public async void Extract_Spreadsheet_Data_v1_FollowupConfiguration_With_Zero_Upstream_Crates()
        {
            var configureUrl = GetTerminalConfigureUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = fixture.Extract_Spreadsheet_Data_v1_Followup_Configuration_Request_ActionDTO_With_Crates();

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

        /////////////
        /// Followup Configuration End
        /////////////

        /////////////
        /// Run Tests Begin
        /////////////

        [Test, Category("Integration.terminalGoogle")]
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException),
            ExpectedMessage = @"{""status"":""terminal_error"",""message"":""No AuthToken provided.""}"
        )]
        public async void Extract_Spreadsheet_Data_v1_Run_No_Auth()
        {
            //Arrange
            var runUrl = GetTerminalRunUrl();
            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();

            //prepare the action DTO with valid target URL
            var actionDTO = HealthMonitor_FixtureData.Extract_Spreadsheet_Data_v1_InitialConfiguration_ActionDTO();
            actionDTO.AuthToken = null;
            //Act
            await HttpPostAsync<ActionDTO, PayloadDTO>(runUrl, actionDTO);
        }

        [Test, Category("Integration.terminalGoogle")]
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException),
            ExpectedMessage = @"{""status"":""terminal_error"",""message"":""No Standard File Handle crate found in upstream.""}"
        )]
        public async void Extract_Spreadsheet_Data_v1_Run_With_Zero_Upstream_Crates()
        {
            //Arrange
            var runUrl = GetTerminalRunUrl();
            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();

            //prepare the action DTO with valid target URL
            var actionDTO = HealthMonitor_FixtureData.Extract_Spreadsheet_Data_v1_InitialConfiguration_ActionDTO();

            //Act
            await HttpPostAsync<ActionDTO, PayloadDTO>(runUrl, actionDTO);
        }

        [Test, Category("Integration.terminalGoogle")]
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException),
            ExpectedMessage = @"{""status"":""terminal_error"",""message"":""The method or operation is not implemented.""}"
        )]
        public async void Extract_Spreadsheet_Data_v1_Run_With_One_Upstream_Crates()
        {
            //Arrange
            var runUrl = GetTerminalRunUrl();
            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();

            //prepare the action DTO with valid target URL
            var actionDTO = HealthMonitor_FixtureData.Extract_Spreadsheet_Data_v1_InitialConfiguration_ActionDTO();
            AddUpstreamCrate(actionDTO, fixture.GetUpstreamCrate(), "Upsteam Crate");

            //Act
            await HttpPostAsync<ActionDTO, PayloadDTO>(runUrl, actionDTO);
        }

        [Test, Category("Integration.terminalGoogle")]
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException),
            ExpectedMessage = @"{""status"":""terminal_error"",""message"":""More than one Standard File Handle crates found upstream.""}"
        )]
        public async void Extract_Spreadsheet_Data_v1_Run_With_Two_Upstream_Crates()
        {
            //Arrange
            var runUrl = GetTerminalRunUrl();
            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            
            //prepare the action DTO with valid target URL
            var actionDTO = HealthMonitor_FixtureData.Extract_Spreadsheet_Data_v1_InitialConfiguration_ActionDTO();
            AddUpstreamCrate(actionDTO, fixture.GetUpstreamCrate(), "Upsteam Crate");
            AddUpstreamCrate(actionDTO, fixture.GetUpstreamCrate(), "Upsteam Crate");
            //Act
            await HttpPostAsync<ActionDTO, PayloadDTO>(runUrl, actionDTO);
        }
        /////////////
        /// Run Tests End
        /////////////



        [Test, Category("Integration.terminalGoogle")]
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException),
            ExpectedMessage = @"{""status"":""terminal_error"",""message"":""One or more errors occurred.""}"
        )]
        public async void Extract_Spreadsheet_Data_v1_Configure_NoActionId()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Extract_Spreadsheet_Data_v1_InitialConfiguration_ActionDTO();
            requestActionDTO.Id = Guid.Empty;

            await HttpPostAsync<ActionDTO, Action>(configureUrl, requestActionDTO);
        }

        /// <summary>
        /// Test run-time without Auth-Token.
        /// </summary>
        [Test, Category("Integration.terminalGoogle")]
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException),
            ExpectedMessage = @"{""status"":""terminal_error"",""message"":""One or more errors occurred.""}"
        )]
        public async void Extract_Spreadsheet_Data_v1_Run_NoAuth()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Extract_Spreadsheet_Data_v1_InitialConfiguration_ActionDTO();
            requestActionDTO.AuthToken = null;

            await HttpPostAsync<ActionDTO, ActionDTO>(configureUrl, requestActionDTO);
        }
    }
}
