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
    public class Extract_Spreadsheet_Data_v1Tests : BaseTerminalIntegrationTest
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
            requestActionDTO.AuthToken = HealthMonitor_FixtureData.Google_AuthToken1();
            var responseActionDTO =
                await HttpPostAsync<ActivityDTO, ActivityDTO>(
                    configureUrl, requestActionDTO
                );

            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            Assert.NotNull(responseActionDTO.CrateStorage.Crates);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            AssertCrateTypes_OnConfiguration(crateStorage);
            AssertControls_OnConfiguration(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());
        }
        private void AssertCrateTypes_OnConfiguration(CrateStorage crateStorage)
        {
            Assert.AreEqual(1, crateStorage.Count);
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count());
        }

        private void AssertControls_OnConfiguration(StandardConfigurationControlsCM controls)
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
        /// <summary>
        /// Spreadsheet with the following structure is passed: {{(1,1),(1,2)},{(2,1),(2,2)}}
        /// Required fields are tested
        /// </summary> 
        [Test, Category("Integration.terminalGoogle")]
        public async void Extract_Spreadsheet_Data_v1_FollowupConfiguration_Row_And_Column_Table()
        {
            var configureUrl = GetTerminalConfigureUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = fixture.Extract_Spreadsheet_Data_v1_Followup_Configuration_Request_ActionDTO_With_Crates();

            ////Act
            fixture.Extract_Spreadsheet_Data_v1_AddPayload(requestActionDTO, "Row_And_Column");

            //As the ActionDTO is preconfigured configure url actually calls the follow up configuration
            var responseActionDTO =
               await HttpPostAsync<ActivityDTO, ActivityDTO>(
                   configureUrl,
                   requestActionDTO
               );

            //Assert
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            Assert.NotNull(responseActionDTO.CrateStorage.Crates);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            Assert.AreEqual(4, crateStorage.Count);
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count());
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardTableDataCM>().Count());
            Assert.IsFalse(crateStorage.CratesOfType<StandardTableDataCM>().Single().Content.FirstRowHeaders);
            
            // Due to performance issue, remove functionalilty to load table contents
            //  Assert.AreEqual("(2,1)", crateStorage.CratesOfType<StandardTableDataCM>().Single().Content.Table[0].Row[0].Cell.Value);
            //Assert.AreEqual("(2,2)", crateStorage.CratesOfType<StandardTableDataCM>().Single().Content.Table[0].Row[1].Cell.Value);
        }
        /// <summary>
        /// Spreadsheet with the following structure is passed: {{(1,1)},{(2,2)}}
        /// Required fields are tested
        /// </summary> 
        [Test, Category("Integration.terminalGoogle")]
        public async void Extract_Spreadsheet_Data_v1_FollowupConfiguration_Column_Only_Table()
        {
            var configureUrl = GetTerminalConfigureUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = fixture.Extract_Spreadsheet_Data_v1_Followup_Configuration_Request_ActionDTO_With_Crates();

            ////Act
            fixture.Extract_Spreadsheet_Data_v1_AddPayload(requestActionDTO, "Column_Only");

            //As the ActionDTO is preconfigured configure url actually calls the follow up configuration
            var responseActionDTO =
               await HttpPostAsync<ActivityDTO, ActivityDTO>(
                   configureUrl,
                   requestActionDTO
               );

            //Assert
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            Assert.NotNull(responseActionDTO.CrateStorage.Crates);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            Assert.AreEqual(4, crateStorage.Count);
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count());
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardTableDataCM>().Count());
            Assert.IsFalse(crateStorage.CratesOfType<StandardTableDataCM>().Single().Content.FirstRowHeaders);

            // Due to performance issue, remove functionalilty to load table contents
           // Assert.AreEqual("(2,1)", crateStorage.CratesOfType<StandardTableDataCM>().Single().Content.Table[0].Row[0].Cell.Value);
        }
        /// <summary>
        /// Spreadsheet with the following structure is passed: {{(1,1),(1,2)}}
        /// Required fields are tested
        /// </summary> 
        [Test, Category("Integration.terminalGoogle")]
        public async void Extract_Spreadsheet_Data_v1_FollowupConfiguration_Row_Only_Table()
        {
            var configureUrl = GetTerminalConfigureUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = fixture.Extract_Spreadsheet_Data_v1_Followup_Configuration_Request_ActionDTO_With_Crates();

            ////Act
            fixture.Extract_Spreadsheet_Data_v1_AddPayload(requestActionDTO, "Row_Only");

            //As the ActionDTO is preconfigured configure url actually calls the follow up configuration
            var responseActionDTO =
               await HttpPostAsync<ActivityDTO, ActivityDTO>(
                   configureUrl,
                   requestActionDTO
               );

            //Assert
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            Assert.NotNull(responseActionDTO.CrateStorage.Crates);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            Assert.AreEqual(3, crateStorage.Count);
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count());
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardTableDataCM>().Count());
            Assert.IsFalse(crateStorage.CratesOfType<StandardTableDataCM>().Single().Content.FirstRowHeaders);
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardTableDataCM>().Single().Content.Table.Count);
        }
        /////////////
        /// Followup Configuration End
        /////////////

        /////////////
        /// Run Tests Begin
        /////////////

        //To be finished after Action is fixed

        /// <summary>
        /// Spreadsheet with the following structure is passed: {{(),()}{(2,1)},{(2,2)}}
        /// Should throw exception
        /// </summary> 
        //[Test, Category("Integration.terminalGoogle")]
        //[ExpectedException(
        //    ExpectedException = typeof(RestfulServiceException),
        //    ExpectedMessage = @"{""status"":""terminal_error"",""message"":""No headers found in the Standard Table Data Manifest.""}"
        //)]
        //public async void Extract_Spreadsheet_Data_v1_FollowupConfiguration_Empty_First_Row()
        //{
        //    var configureUrl = GetTerminalConfigureUrl();
        //    var runUrl = GetTerminalRunUrl();
        //    HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
        //    var requestActionDTO = fixture.Extract_Spreadsheet_Data_v1_Followup_Configuration_Request_ActionDTO_With_Crates();

        //    ////Act
        //    fixture.Extract_Spreadsheet_Data_v1_AddPayload(requestActionDTO, "Empty_First_Row");

        //    //As the ActionDTO is preconfigured configure url actually calls the follow up configuration
        //    var responseActionDTO = await HttpPostAsync<ActionDTO, ActionDTO>(configureUrl, requestActionDTO);
        //    await HttpPostAsync<ActionDTO, PayloadDTO>(runUrl, responseActionDTO);
        //}


        /// <summary>
        /// Run ActionType with no AuthToken provided throws exception.
        /// </summary>
        [Test, Category("Integration.terminalGoogle")]
        public async void Extract_Spreadsheet_Data_v1_Run_No_Auth()
        {
            //Arrange
            var runUrl = GetTerminalRunUrl();
            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();

            //prepare the action DTO with valid target URL
            var activityDTO = HealthMonitor_FixtureData.Extract_Spreadsheet_Data_v1_InitialConfiguration_ActionDTO();
            activityDTO.AuthToken = null;

            AddOperationalStateCrate(activityDTO, new OperationalStateCM());
            //Act
            var payload = await HttpPostAsync<ActivityDTO, PayloadDTO>(runUrl, activityDTO);
            CheckIfPayloadHasNeedsAuthenticationError(payload);
        }
        /// <summary>
        /// Zero Upstream Crates throws exception.
        /// </summary>
        [Test, Category("Integration.terminalGoogle")]
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException),
            ExpectedMessage = @"{""status"":""terminal_error"",""message"":""No Standard File Handle crate found in upstream.""}",
            MatchType = MessageMatch.Contains
        )]
        public async void Extract_Spreadsheet_Data_v1_Run_With_Zero_Upstream_Crates()
        {
            //Arrange
            var runUrl = GetTerminalRunUrl();

            //prepare the action DTO
            var activityDTO = HealthMonitor_FixtureData.Extract_Spreadsheet_Data_v1_InitialConfiguration_ActionDTO();
            AddOperationalStateCrate(activityDTO, new OperationalStateCM());
            //Act
            await HttpPostAsync<ActivityDTO, PayloadDTO>(runUrl, activityDTO);
        }
        /// <summary>
        /// One Upstream Crate throws NonImplementedException.
        /// </summary>
        [Test, Category("Integration.terminalGoogle")]
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException),
            ExpectedMessage = @"{""status"":""terminal_error"",""message"":""The method or operation is not implemented.""}",
            MatchType = MessageMatch.Contains
        )]
        public async void Extract_Spreadsheet_Data_v1_Run_With_One_Upstream_Crates()
        {
            //Arrange
            var runUrl = GetTerminalRunUrl();
            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();

            //prepare the action DTO with valid target URL
            var activityDTO = HealthMonitor_FixtureData.Extract_Spreadsheet_Data_v1_InitialConfiguration_ActionDTO();
            AddUpstreamCrate(activityDTO, fixture.GetUpstreamCrate(), "Upsteam Crate");
            AddOperationalStateCrate(activityDTO, new OperationalStateCM());
            //Act
            await HttpPostAsync<ActivityDTO, PayloadDTO>(runUrl, activityDTO);
        }
        /// <summary>
        /// Two Upstream Crate throw exception.
        /// </summary>
        [Test, Category("Integration.terminalGoogle")]
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException),
            ExpectedMessage = @"{""status"":""terminal_error"",""message"":""More than one Standard File Handle crates found upstream.""}",
            MatchType = MessageMatch.Contains
        )]
        public async void Extract_Spreadsheet_Data_v1_Run_With_Two_Upstream_Crates()
        {
            //Arrange
            var runUrl = GetTerminalRunUrl();
            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();

            //prepare the action DTO with valid target URL
            var activityDTO = HealthMonitor_FixtureData.Extract_Spreadsheet_Data_v1_InitialConfiguration_ActionDTO();
            AddUpstreamCrate(activityDTO, fixture.GetUpstreamCrate(), "Upsteam Crate");
            AddUpstreamCrate(activityDTO, fixture.GetUpstreamCrate(), "Upsteam Crate");
            AddOperationalStateCrate(activityDTO, new OperationalStateCM());
            //Act
            await HttpPostAsync<ActivityDTO, PayloadDTO>(runUrl, activityDTO);
        }
        /// <summary>
        /// Test run-time without Auth-Token.
        /// </summary>
        [Test, Category("Integration.terminalGoogle")]
        public async void Extract_Spreadsheet_Data_v1_Run_NoAuth()
        {
            var runUrl = GetTerminalRunUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Extract_Spreadsheet_Data_v1_InitialConfiguration_ActionDTO();
            requestActionDTO.AuthToken = null;
            AddOperationalStateCrate(requestActionDTO, new OperationalStateCM());
            var payload = await HttpPostAsync<ActivityDTO, PayloadDTO>(runUrl, requestActionDTO);
            CheckIfPayloadHasNeedsAuthenticationError(payload);
        }
        /////////////
        /// Run Tests End
        /////////////
    }
}
