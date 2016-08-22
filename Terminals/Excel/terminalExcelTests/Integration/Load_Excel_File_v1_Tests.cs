using System;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.DataTransferObjects.Helpers;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using Fr8.Testing.Integration;
using NUnit.Framework;
using StructureMap;
using terminalExcelTests.Fixtures;
using terminalExcel.Activities;

namespace terminalExcelTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    [Category("terminalExcel.Integration")]
    public class Load_Excel_File_v1_Tests : BaseTerminalIntegrationTest
    {
        public ICrateManager _crateManager;

        [SetUp]
        public void SetUp()
        {
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
        }

        public override string TerminalName => "terminalExcel";

        private async Task<ActivityDTO> ConfigureFollowUp(bool setFileName = false, string fileUri = null, ActivityDTO existingActivity = null)
        {
            if (string.IsNullOrEmpty(fileUri))
            {
                fileUri = HealthMonitor_FixtureData.GetFilePath();
            }

            var configureUrl = GetTerminalConfigureUrl();
            var dataDTO = HealthMonitor_FixtureData.Load_Excel_File_v1_InitialConfiguration_Fr8DataDTO(Guid.NewGuid());
            if (existingActivity == null)
            {
                existingActivity = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO);
            }

            if (setFileName)
            {
                using (var storage = _crateManager.GetUpdatableStorage(existingActivity))
                {
                    var activityUi = new Load_Excel_File_v1.ActivityUi();
                    var controlsCrate = _crateManager.GetStorage(existingActivity).FirstCrate<StandardConfigurationControlsCM>();
                    activityUi.SyncWith(controlsCrate.Content);
                    activityUi.FilePicker.Value = fileUri;
                    storage.ReplaceByLabel(Fr8.Infrastructure.Data.Crates.Crate.FromContent(controlsCrate.Label, new StandardConfigurationControlsCM(activityUi.Controls.ToArray())));
                }
            }
            dataDTO.ActivityDTO = existingActivity;
            return await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO);
        }

        [Test]
        public async Task Load_Excel_File_v1_Initial_Configuration_Check_Crate_Structure()
        {
            // Arrange
            var configureUrl = GetTerminalConfigureUrl();
            var requestActionDTO = HealthMonitor_FixtureData.Load_Excel_File_v1_InitialConfiguration_Fr8DataDTO(Guid.NewGuid());

            // Act
            var responseActionDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, requestActionDTO);

            // Assert
            Assert.NotNull(responseActionDTO, "Response from initial configuration request is null");
            Assert.NotNull(responseActionDTO.CrateStorage, "Response from initial configuration request doesn't contain crate storage");
            Assert.NotNull(responseActionDTO.CrateStorage.Crates, "Response from initial configuration request doesn't contain crates in storage");

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(), "Activity storage doesn't contain configuration controls");
            Assert.AreEqual(1, crateStorage.CratesOfType<CrateDescriptionCM>().Count(), "Activity storage doesn't contain description of runtime available crates");
        }

        [Test]
        public async Task Load_Excel_File_v1_FollowUp_Configuration_WithFileSelected_CheckCrateStructure()
        {
            // Act
            var responseFollowUpActionDTO = await ConfigureFollowUp(true);

            // Assert
            Assert.NotNull(responseFollowUpActionDTO, "Response from followup configuration request is null");
            Assert.NotNull(responseFollowUpActionDTO.CrateStorage, "Response from followup configuration request doesn't contain crate storage");
            Assert.NotNull(responseFollowUpActionDTO.CrateStorage.Crates, "Response from followup configuration request doesn't contain crates in storage");

            var crateStorage = _crateManager.GetStorage(responseFollowUpActionDTO);
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(), "Activity storage doesn't contain configuration controls");
            Assert.AreEqual(1, crateStorage.CratesOfType<CrateDescriptionCM>().Count(), "Activity storage doesn't contain description of runtime available crates");
          /*  Assert.AreEqual(1,
                            crateStorage.CratesOfType<FieldDescriptionsCM>().Count(x => x.Availability == AvailabilityType.Always),
                            "Activity storage doesn't contain crate with column headers that is avaialbe both at design time and runtime");
        */
        }

        [Test]
        public async Task Load_Excel_File_v1_FollowUp_Configuration_WithFileSelected_OneRow_CheckCrateStructure()
        {
            // Act
            var responseFollowUpActionDTO = await ConfigureFollowUp(true, HealthMonitor_FixtureData.GetFilePath_OneRowWithWithHeader());

            // Assert
            Assert.NotNull(responseFollowUpActionDTO, "Response from followup configuration request is null");
            Assert.NotNull(responseFollowUpActionDTO.CrateStorage, "Response from followup configuration request doesn't contain crate storage");
            Assert.NotNull(responseFollowUpActionDTO.CrateStorage.Crates, "Response from followup configuration request doesn't contain crates in storage");

            var crateStorage = _crateManager.GetStorage(responseFollowUpActionDTO);
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(), "Activity storage doesn't contain configuration controls");
            Assert.AreEqual(1, crateStorage.CratesOfType<CrateDescriptionCM>().Count(), "Activity storage doesn't contain description of runtime available crates");
           /* Assert.AreEqual(3, crateStorage.CratesOfType<FieldDescriptionsCM>().Count(), "Although one-row table is supplied, there seems to be no FieldDescriptionsCM crate with fields from the first row");
            Assert.AreEqual(2,
                            crateStorage.CratesOfType<FieldDescriptionsCM>().Count(x => x.Availability == AvailabilityType.Always),
                            "Activity storage doesn't contain crate with column headers that is avaialbe both at design time and runtime");
                            */
            // Load file with multiple rows: crate with extracted fields must disappear
            responseFollowUpActionDTO = await ConfigureFollowUp(true, null, responseFollowUpActionDTO);
            crateStorage = _crateManager.GetStorage(responseFollowUpActionDTO);
           // Assert.AreEqual(2, crateStorage.CratesOfType<FieldDescriptionsCM>().Count(), "Although a multi-row table has been uploaded, the crate with extracted fields did not seem to have disappeared");
        }

        [Test]
        public async Task Load_Excel_File_v1_FollowUp_Configuration_WithoutFileSet_CheckCrateStructure()
        {
            // Act
            var responseFollowUpActionDTO = await ConfigureFollowUp();

            // Assert
            Assert.NotNull(responseFollowUpActionDTO, "Response from followup configuration request is null");
            Assert.NotNull(responseFollowUpActionDTO.CrateStorage, "Response from followup configuration request doesn't contain crate storage");
            Assert.NotNull(responseFollowUpActionDTO.CrateStorage.Crates, "Response from followup configuration request doesn't contain crates in storage");

            var crateStorage = _crateManager.GetStorage(responseFollowUpActionDTO);
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(), "Activity storage doesn't contain configuration controls");
            Assert.AreEqual(1, crateStorage.CratesOfType<CrateDescriptionCM>().Count(), "Activity storage doesn't contain description of runtime available crates");
            /* Assert.AreEqual(0,
                            crateStorage.CratesOfType<FieldDescriptionsCM>().Count(x => x.Availability == AvailabilityType.Always),
                            "Activity storage shoudn't contain crate with column headers when file is not selected");
            */
        }

        [Test]
        public async Task Load_Excel_File_v1_Run_WhenFileIsNotSelected_ResponseContainsErrorMessage()
        {
            var activityDTO = await ConfigureFollowUp();
            var runUrl = GetTerminalRunUrl();
            var dataDTO = new Fr8DataDTO { ActivityDTO = activityDTO };
            dataDTO.ActivityDTO = activityDTO;
            AddOperationalStateCrate(dataDTO, new OperationalStateCM());

            var responsePayloadDTO = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);

            var operationalState = Crate.GetStorage(responsePayloadDTO).FirstCrate<OperationalStateCM>().Content;
            
            //extract current error message from current activity response
            ErrorDTO errorMessage;
            operationalState.CurrentActivityResponse.TryParseErrorDTO(out errorMessage);

            Assert.AreEqual(ActivityErrorCode.DESIGN_TIME_DATA_MISSING.ToString(), errorMessage.ErrorCode, "Operational state should contain error response when file is not selected");
        }

        [Test]
        public async Task Load_Excel_File_v1_Run_WhenFileIsSelected_ResponseContainsTableData()
        {
            var activityDTO = await ConfigureFollowUp(true);
            var runUrl = GetTerminalRunUrl();
            var dataDTO = new Fr8DataDTO { ActivityDTO = activityDTO };
            AddOperationalStateCrate(dataDTO, new OperationalStateCM());

            var responsePayloadDTO = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);

            Assert.AreEqual(1, _crateManager.GetStorage(responsePayloadDTO).CratesOfType<StandardTableDataCM>().Count(), "Reponse payload doesn't contain table data from file");
        }

        [Test]
        public async Task Load_Excel_File_v1_Run_WhenFileIsSelected_OneRow_ResponseContainsExtractedFields()
        {
            var activityDTO = await ConfigureFollowUp(true, HealthMonitor_FixtureData.GetFilePath_OneRowWithWithHeader());
            var runUrl = GetTerminalRunUrl();
            var dataDTO = new Fr8DataDTO { ActivityDTO = activityDTO };
            AddOperationalStateCrate(dataDTO, new OperationalStateCM());

            var responsePayloadDTO = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);

            var payload = _crateManager.GetStorage(responsePayloadDTO).CratesOfType<StandardPayloadDataCM>();
            Assert.AreEqual(1, payload.Count(), "Reponse payload doesn't contain extracted fields data from file");
            Assert.AreEqual(4, payload.First().Content.PayloadObjects[0].PayloadObject.Count(), "Reponse payload doesn't contain extracted fields data from file");
        }

        [Test]
        public async Task Load_Table_Data_Activate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalActivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = HealthMonitor_FixtureData.Load_Excel_File_v1_InitialConfiguration_Fr8DataDTO(Guid.NewGuid());
            using (var storage = _crateManager.GetUpdatableStorage(requestActionDTO.ActivityDTO))
            {
                storage.Add(Fr8.Infrastructure.Data.Crates.Crate.FromContent(ExplicitTerminalActivity.ConfigurationControlsLabel, new StandardConfigurationControlsCM(new Load_Excel_File_v1.ActivityUi().Controls.ToArray())));
            }

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
        public async Task Load_Table_Data_Deactivate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalDeactivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = HealthMonitor_FixtureData.Load_Excel_File_v1_InitialConfiguration_Fr8DataDTO(Guid.NewGuid());
            using (var storage = _crateManager.GetUpdatableStorage(requestActionDTO.ActivityDTO))
            {
                storage.Add(Fr8.Infrastructure.Data.Crates.Crate.FromContent(ExplicitTerminalActivity.ConfigurationControlsLabel, new StandardConfigurationControlsCM(new Load_Excel_File_v1.ActivityUi().Controls.ToArray())));
            }

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
