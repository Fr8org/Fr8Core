using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Testing.Integration;
using NUnit.Framework;

namespace terminalGoogleTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    public class Get_Google_Sheet_Data_v1Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName => "terminalGoogle";

        /////////////
        /// Initial Configuration Tests Begin
        /////////////
        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test, Category("Integration.terminalGoogle")]
        public async Task Get_Google_Sheet_Data_Initial_Configuration_Check_Crate_Structure()
        {
            var configureUrl = GetTerminalConfigureUrl();
            var dataDTO = HealthMonitor_FixtureData.Get_Google_Sheet_Data_v1_InitialConfiguration_Fr8DataDTO();
            dataDTO.ActivityDTO.AuthToken = HealthMonitor_FixtureData.Google_AuthToken1();
            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl, dataDTO
                );

            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            AssertCrateTypes_OnConfiguration(crateStorage);
        }

        private void AssertCrateTypes_OnConfiguration(ICrateStorage crateStorage)
        {
            Assert.AreEqual(1, crateStorage.Count);
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count());
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
        public async Task Get_Google_Sheet_Data_v1_FollowupConfiguration_Row_And_Column_Table()
        {
            var configureUrl = GetTerminalConfigureUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = fixture.Get_Google_Sheet_Data_v1_Followup_Configuration_Request_ActivityDTO_With_Crates();

            ////Act
            fixture.Get_Google_Sheet_Data_v1_AddPayload(requestActionDTO, "Row_And_Column");
            var dataDTO = new Fr8DataDTO { ActivityDTO = requestActionDTO };
            //As the ActionDTO is preconfigured configure url actually calls the follow up configuration
            var responseActionDTO =
               await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                   configureUrl,
                   dataDTO
               );

            //Assert
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            Assert.AreEqual(1, crateStorage.Count);
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count());

            // Due to performance issue, remove functionalilty to load table contents
            //  Assert.AreEqual("(2,1)", crateStorage.CratesOfType<StandardTableDataCM>().Single().Content.Table[0].Row[0].Cell.Value);
            //Assert.AreEqual("(2,2)", crateStorage.CratesOfType<StandardTableDataCM>().Single().Content.Table[0].Row[1].Cell.Value);
        }

        /// <summary>
        /// Spreadsheet with the following structure is passed: {{(1,1)},{(2,2)}}
        /// Required fields are tested
        /// </summary> 
        [Test, Category("Integration.terminalGoogle")]
        public async Task Get_Google_Sheet_Data_v1_FollowupConfiguration_Column_Only_Table()
        {
            var configureUrl = GetTerminalConfigureUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = fixture.Get_Google_Sheet_Data_v1_Followup_Configuration_Request_ActivityDTO_With_Crates();

            //Act
            fixture.Get_Google_Sheet_Data_v1_AddPayload(requestActionDTO, "Column_Only");
            var dataDTO = new Fr8DataDTO { ActivityDTO = requestActionDTO };
            //As the ActionDTO is preconfigured configure url actually calls the follow up configuration
            var responseActionDTO =
               await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                   configureUrl,
                   dataDTO
               );

            //Assert
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            Assert.AreEqual(1, crateStorage.Count);
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count());

            // Due to performance issue, remove functionalilty to load table contents
            // Assert.AreEqual("(2,1)", crateStorage.CratesOfType<StandardTableDataCM>().Single().Content.Table[0].Row[0].Cell.Value);
        }

        [Test, Category("Integration.terminalGoogle")]
        public async Task Get_Google_Sheet_Data_v1_FollowupConfiguration_OneRow_Table()
        {
            var configureUrl = GetTerminalConfigureUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = fixture.Get_Google_Sheet_Data_v1_Followup_Configuration_Request_ActivityDTO_With_Crates();

            //Act
            fixture.Get_Google_Sheet_Data_v1_AddPayload(requestActionDTO, "OneRow_WithHeader");
            var dataDTO = new Fr8DataDTO { ActivityDTO = requestActionDTO };
            //As the ActionDTO is preconfigured configure url actually calls the follow up configuration
            var responseActionDTO =
               await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                   configureUrl,
                   dataDTO
               );

            //Assert
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);

            Assert.AreEqual(4, crateStorage.Count);
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count());
            Assert.AreEqual(1, crateStorage.CratesOfType<CrateDescriptionCM>().Count());
            Assert.AreEqual(1, crateStorage.FirstCrate<CrateDescriptionCM>().Content.CrateDescriptions.Count);
            Assert.AreEqual(4, crateStorage.FirstCrate<CrateDescriptionCM>().Content.CrateDescriptions[0].Fields.Count);
        }

        /// <summary>
        /// Spreadsheet with the following structure is passed: {{(1,1),(1,2)}}
        /// Required fields are tested
        /// </summary> 
        [Test, Category("Integration.terminalGoogle")]
        public async Task Get_Google_Sheet_Data_v1_FollowupConfiguration_Row_Only_Table()
        {
            var configureUrl = GetTerminalConfigureUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = fixture.Get_Google_Sheet_Data_v1_Followup_Configuration_Request_ActivityDTO_With_Crates();

            ////Act
            fixture.Get_Google_Sheet_Data_v1_AddPayload(requestActionDTO, "Row_Only");
            var dataDTO = new Fr8DataDTO { ActivityDTO = requestActionDTO };
            //As the ActionDTO is preconfigured configure url actually calls the follow up configuration
            var responseActionDTO =
               await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                   configureUrl,
                   dataDTO
               );

            //Assert
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            Assert.AreEqual(1, crateStorage.Count);
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count());
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
        //public async Task Get_Google_Sheet_Data_v1_FollowupConfiguration_Empty_First_Row()
        //{
        //    var configureUrl = GetTerminalConfigureUrl();
        //    var runUrl = GetTerminalRunUrl();
        //    HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
        //    var requestActionDTO = fixture.Get_Google_Sheet_Data_v1_Followup_Configuration_Request_ActionDTO_With_Crates();

        //    ////Act
        //    fixture.Get_Google_Sheet_Data_v1_AddPayload(requestActionDTO, "Empty_First_Row");

        //    //As the ActionDTO is preconfigured configure url actually calls the follow up configuration
        //    var responseActionDTO = await HttpPostAsync<ActionDTO, ActionDTO>(configureUrl, requestActionDTO);
        //    await HttpPostAsync<ActionDTO, PayloadDTO>(runUrl, responseActionDTO);
        //}


        /// <summary>
        /// Run ActionType with no AuthToken provided throws exception.
        /// </summary>
        [Test, Category("Integration.terminalGoogle")]
        public async Task Get_Google_Sheet_Data_v1_Run_No_Auth()
        {
            //Arrange
            var runUrl = GetTerminalRunUrl();

            //prepare the action DTO with valid target URL
            var dataDTO = HealthMonitor_FixtureData.Get_Google_Sheet_Data_v1_InitialConfiguration_Fr8DataDTO();
            dataDTO.ActivityDTO.AuthToken = null;

            AddOperationalStateCrate(dataDTO, new OperationalStateCM());
            //Act
            var payload = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);
            CheckIfPayloadHasNeedsAuthenticationError(payload);
        }
        /// <summary>
        /// Zero Upstream Crates throws exception.
        /// </summary>
        //[Test, Category("Integration.terminalGoogle")]
        //[ExpectedException(
        //    ExpectedException = typeof(RestfulServiceException),
        //    ExpectedMessage = @"{""status"":""terminal_error"",""message"":""No Standard File Handle crate found in upstream.""}",
        //    MatchType = MessageMatch.Contains
        //)]
        //public async Task Get_Google_Sheet_Data_v1_Run_With_Zero_Upstream_Crates()
        //{
        //    //Arrange
        //    var runUrl = GetTerminalRunUrl();
        //    HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();

        //    //prepare the action DTO
        //    var dataDTO = HealthMonitor_FixtureData.Get_Google_Sheet_Data_v1_InitialConfiguration_Fr8DataDTO();
        //    AddOperationalStateCrate(dataDTO.ActivityDTO, new OperationalStateCM());

        //    //Act
        //    await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);
        //}
        /// <summary>
        /// One Upstream Crate throws NonImplementedException.
        /// </summary>
        //[Test, Category("Integration.terminalGoogle")]
        //[ExpectedException(
        //    ExpectedException = typeof(RestfulServiceException),
        //    ExpectedMessage = @"{""status"":""terminal_error"",""message"":""The method or operation is not implemented.""}",
        //    MatchType = MessageMatch.Contains
        //)]
        //public async Task Get_Google_Sheet_Data_v1_Run_With_One_Upstream_Crates()
        //{
        //    //Arrange
        //    var runUrl = GetTerminalRunUrl();
        //    HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();

        //    //prepare the action DTO with valid target URL
        //    var dataDTO = HealthMonitor_FixtureData.Get_Google_Sheet_Data_v1_InitialConfiguration_Fr8DataDTO();
        //    AddUpstreamCrate(dataDTO.ActivityDTO, fixture.GetUpstreamCrate(), "Upsteam Crate");
        //    AddOperationalStateCrate(dataDTO.ActivityDTO, new OperationalStateCM());

        //    //Act
        //    await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);
        //}
        /// <summary>
        /// Two Upstream Crate throw exception.
        /// </summary>
        //[Test, Category("Integration.terminalGoogle")]
        //[ExpectedException(
        //    ExpectedException = typeof(RestfulServiceException),
        //    ExpectedMessage = @"{""status"":""terminal_error"",""message"":""More than one Standard File Handle crates found upstream.""}",
        //    MatchType = MessageMatch.Contains
        //)]
        //public async Task Get_Google_Sheet_Data_v1_Run_With_Two_Upstream_Crates()
        //{
        //    //Arrange
        //    var runUrl = GetTerminalRunUrl();
        //    HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();

        //    //prepare the action DTO with valid target URL
        //    var dataDTO = HealthMonitor_FixtureData.Get_Google_Sheet_Data_v1_InitialConfiguration_Fr8DataDTO();
        //    AddUpstreamCrate(dataDTO.ActivityDTO, fixture.GetUpstreamCrate(), "Upsteam Crate");
        //    AddUpstreamCrate(dataDTO.ActivityDTO, fixture.GetUpstreamCrate(), "Upsteam Crate");
        //    AddOperationalStateCrate(dataDTO.ActivityDTO, new OperationalStateCM());

        //    //Act
        //    await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);
        //}
        /// <summary>
        /// This test verifies that the crate label is updated in accord with spreadsheet name
        /// </summary>
        /// 

        [Test, Category("Integration.terminalGoogle")]
        public async Task Get_Google_Sheet_Data_v1_Run_Sets_Label_Based_On_Spreadsheet_Name()
        {
            /*
            //Arrange
            var runUrl = GetTerminalRunUrl();
            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = fixture.Get_Google_Sheet_Data_v1_Followup_Configuration_Request_ActivityDTO_With_Crates();
            fixture.Get_Google_Sheet_Data_v1_AddPayload(requestActionDTO, "Row_Only");
            var dataDTO = new Fr8DataDTO { ActivityDTO = requestActionDTO };
            AddOperationalStateCrate(dataDTO, new OperationalStateCM());
            ////Act
            var payload = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);
            var storage = Crate.GetStorage(payload);
            var tableDataCrate = storage.CratesOfType<StandardTableDataCM>().Single();
            ////Assert
            Assert.AreEqual("Table Generated From Google Sheet Data", tableDataCrate.Label);
            */
        }

        [Test, Category("Integration.terminalGoogle")]
        public async Task Get_Google_Sheet_Data_v1_Run_OneRow_ResponseContainsExtractedFields()
        {
            //Arrange
            var runUrl = GetTerminalRunUrl();
            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = fixture.Get_Google_Sheet_Data_v1_Followup_Configuration_Request_ActivityDTO_With_Crates();
            fixture.Get_Google_Sheet_Data_v1_AddPayload(requestActionDTO, "OneRow_WithHeader");
            var dataDTO = new Fr8DataDTO { ActivityDTO = requestActionDTO };
            AddOperationalStateCrate(dataDTO, new OperationalStateCM());
            ////Act
            var response = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);
            var storage = Crate.GetStorage(response);
            var tableDataCrate = storage.CratesOfType<StandardTableDataCM>().Single();
            ////Assert
            Assert.AreEqual("Spreadsheet Data from \"OneRow_WithHeader\"", tableDataCrate.Label);

        }

        /////////////
        /// Run Tests End
        /////////////
    }
}
