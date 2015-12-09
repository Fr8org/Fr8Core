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
using Hub.StructureMap;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using StructureMap;
using terminalExcelTests.Fixtures;

namespace terminalExcelTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    [Category("terminalExcel.Integration")]
    public class Load_Excel_File_v1_Tests : BaseHealthMonitorTest
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
            get { return "terminalExcel"; }
        }

        private void AssertCrateTypes(CrateStorage crateStorage)
        {
            Assert.AreEqual(crateStorage.Count, 2);
            Assert.AreEqual(crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(), 1);
            Assert.AreEqual(crateStorage.CratesOfType<StandardDesignTimeFieldsCM>().Count(x => x.Label == "Select Excel File"), 1);
        }

        private void AssertControls(StandardConfigurationControlsCM controls)
        {
            Assert.AreEqual(controls.Controls.Count, 2);

            // Assert that first control is a FilePicker 
            Assert.IsTrue(controls.Controls[0] is FilePicker);
            Assert.AreEqual("select_file", controls.Controls[0].Name);
            Assert.AreEqual("Select an Excel file", controls.Controls[0].Label);
            Assert.AreEqual(controls.Controls[0].Events.Count, 1);
            Assert.AreEqual("onChange", controls.Controls[0].Events[0].Name);
            Assert.AreEqual("requestConfig", controls.Controls[0].Events[0].Handler);

            // Assert that second control is a TextBlock
            Assert.IsTrue(controls.Controls[1] is TextBlock);
            Assert.AreEqual(controls.Controls[1].Name, null);
        }

        private async Task<ActionDTO> ConfigureFollowUp()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Load_Table_Data_v1_InitialConfiguration_ActionDTO(Guid.NewGuid());

            var responseActionDTO = await HttpPostAsync<ActionDTO, ActionDTO>(configureUrl, requestActionDTO);

            var storage = _crateManager.GetStorage(responseActionDTO);

            using (var updater = _crateManager.UpdateStorage(requestActionDTO))
            {
                updater.CrateStorage = storage;
            }

            return await HttpPostAsync<ActionDTO, ActionDTO>(configureUrl, requestActionDTO);
        }

        private void AssertFollowUpCrateTypes(CrateStorage crateStorage)
        {
            Assert.AreEqual(crateStorage.Count, 2);

            Assert.AreEqual(crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(x => x.Label == "Configuration_Controls"), 1);
            Assert.AreEqual(crateStorage.CratesOfType<StandardDesignTimeFieldsCM>().Count(x => x.Label == "Select Excel File"), 1);
        }

        [Test]
        public async void Load_Table_Data_v1_Initial_Configuration_Check_Crate_Structure()
        {
            // Arrange
            var configureUrl = GetTerminalConfigureUrl();
            var requestActionDTO = HealthMonitor_FixtureData.Load_Table_Data_v1_InitialConfiguration_ActionDTO(Guid.NewGuid());

            // Act
            var responseActionDTO = await HttpPostAsync<ActionDTO, ActionDTO>(configureUrl, requestActionDTO);

            // Assert
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            Assert.NotNull(responseActionDTO.CrateStorage.Crates);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            AssertCrateTypes(crateStorage);
            AssertControls(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(RestfulServiceException),
            ExpectedMessage = @"{""status"":""terminal_error"",""message"":""One or more errors occurred.""}"
        )]
        public async void Load_Table_Data_v1_Initial_Configuration_GuidEmpty()
        {
            // Arrange
            var configureUrl = GetTerminalConfigureUrl();
            var requestActionDTO = HealthMonitor_FixtureData.Load_Table_Data_v1_InitialConfiguration_ActionDTO(Guid.Empty);

            // Act
            await HttpPostAsync<ActionDTO, JToken>(configureUrl, requestActionDTO);
        }

        /// <summary>
        /// Validate correct crate-storage structure in follow-up configuration response.
        /// </summary>
        [Test]
        public async void Load_Table_Data_v1_FollowUp_Configuration_Check_Crate_Structure()
        {
            // Act
            var responseFollowUpActionDTO = await ConfigureFollowUp();

            // Assert
            Assert.NotNull(responseFollowUpActionDTO);
            Assert.NotNull(responseFollowUpActionDTO.CrateStorage);
            Assert.NotNull(responseFollowUpActionDTO.CrateStorage.Crates);

            var crateStorage = Crate.FromDto(responseFollowUpActionDTO.CrateStorage);
            AssertFollowUpCrateTypes(crateStorage);
            AssertControls(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());
        }

        [Test]
        public async void Load_Table_Data_v1_Run_()
        {
            // Arrange
            var runUrl = GetTerminalRunUrl();

            var actionDTO = HealthMonitor_FixtureData.Load_Table_Data_v1_InitialConfiguration_ActionDTO(Guid.NewGuid());
            AddPayloadCrate(
                actionDTO,
                new StandardTableDataCM()
                {
                    FirstRowHeaders = true,
                    Table = new List<TableRowDTO>() { new TableRowDTO { Row = new List<TableCellDTO>() {TableCellDTO.Create("key", "value") } } }
                }
            );

            // Act
            var responsePayloadDTO = await HttpPostAsync<ActionDTO, PayloadDTO>(runUrl, actionDTO);

            // Assert
            var crateStorage = Crate.GetStorage(responsePayloadDTO);
            Assert.AreEqual(crateStorage.CrateContentsOfType<StandardPayloadDataCM>(x => x.Label == "Excel Data").Count(), 1);
        }
    }
}
