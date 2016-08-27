using System.Linq;
using Fr8.Testing.Integration;
using NUnit.Framework;
using terminalAzureTests.Fixtures;
using Fr8.Testing.Unit.Fixtures;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;

namespace terminalAzureTests.Integration
{
    [Explicit]
    public class Write_To_Sql_Server_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalAzure"; }
        }

        private void AssertConfigureControls(StandardConfigurationControlsCM control)
        {
            //Now there are three ControlsDefinitionDTO: TextBox for connnection string, Button "Continue" and dropDown to select table
            Assert.AreEqual(3, control.Controls.Count);

            // Assert that first control is a TextBox 
            // with Label == "SQL Connection String"
            // with Name == "connection_string"
            Assert.IsTrue(control.Controls[0] is TextBox);
            Assert.AreEqual("SQL Connection String", control.Controls[0].Label);
            Assert.AreEqual("connection_string", control.Controls[0].Name);
        }

        private void AssertConfigureCrate(ICrateStorage crateStorage)
        {
            Assert.AreEqual(1, crateStorage.Count);
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count());

            AssertConfigureControls(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());
        }

        private Crate CreateConnectionStringCrate()
        {
            var control = FixtureData.TestConnectionString2();
            control.Name = "connection_string";
            control.Label = "SQL Connection String";

            return PackControlsCrate(control);
        }

        private Crate<StandardConfigurationControlsCM> PackControlsCrate(params ControlDefinitionDTO[] controlsList)
        {
            return Crate<StandardConfigurationControlsCM>.FromContent("Configuration_Controls", new StandardConfigurationControlsCM(controlsList));
        }

        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test]
        public async Task Write_To_Sql_Server_Initial_Configuration_Check_Crate_Structure()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Write_To_Sql_Server_v1_InitialConfiguration_Fr8DataDTO();

            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                );

            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            AssertConfigureCrate(crateStorage);
        }

        /// <summary>
        /// Validate correct crate-storage structure in follow-up configuration response 
        /// </summary>
        [Test]
        public async Task Write_To_Sql_Server_FollowUp_Configuration_Check_Crate_Structure()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var fr8DataDTO = HealthMonitor_FixtureData.Write_To_Sql_Server_v1_InitialConfiguration_Fr8DataDTO();

            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    fr8DataDTO
                );


            var storage = Crate.GetStorage(responseActionDTO);

            var controlDefinitionDTO =
                storage.CratesOfType<StandardConfigurationControlsCM>()
                    .Select(x => x.Content.FindByName("connection_string")).ToArray();

            controlDefinitionDTO[0].Value = FixtureData.TestConnectionString2().Value;

            using (var updatableStorage = Crate.GetUpdatableStorage(responseActionDTO))
            {
                updatableStorage.Replace(storage);
            }
            fr8DataDTO.ActivityDTO = responseActionDTO;
            responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    fr8DataDTO
                );

            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);

            Assert.AreEqual(1, crateStorage.Count);
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count());

            AssertConfigureControls(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());
        }

        /// <summary>
        /// Validate correct crate-storage structure in follow-up configuration response 
        /// with incorrect connection string
        /// </summary>
        [Test]
        public async Task Write_To_Sql_Server_FollowUp_Configuration_Check_Crate_Structure_Incorrect_ConnectionString()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var fr8DataDTO = HealthMonitor_FixtureData.Write_To_Sql_Server_v1_InitialConfiguration_Fr8DataDTO();

            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    fr8DataDTO
                );


            var storage = Crate.GetStorage(responseActionDTO);

            var controlDefinitionDTO =
                storage.CratesOfType<StandardConfigurationControlsCM>()
                    .Select(x => x.Content.FindByName("connection_string")).ToArray();
            //provide incorrect connection string
            controlDefinitionDTO[0].Value = FixtureData.TestConnectionString3().Value;

            using (var updatableStorage = Crate.GetUpdatableStorage(responseActionDTO))
            {
                updatableStorage.Replace(storage);
            }
            fr8DataDTO.ActivityDTO = responseActionDTO;
            responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    fr8DataDTO
                );

            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            //There will be no DesignTimeCrate only Configuration crate
            Assert.AreEqual(1, crateStorage.Count);
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count());
            var controls = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();
            AssertConfigureControls(controls);
            //Check that Error message is shown
            var connStringTextBox = (TextBox)controls.Controls[0];
            Assert.AreEqual("Incorrect Connection String", connStringTextBox.Value);
        }

        /// <summary>
        /// Test run-time for action Run().
        /// </summary>
        [Test]
        public async Task Write_To_Sql_Server_Run()
        {
            var runUrl = GetTerminalRunUrl();

            var fr8DataDTO = HealthMonitor_FixtureData.Write_To_Sql_Server_v1_InitialConfiguration_Fr8DataDTO();

            using (var crateStorage = Crate.GetUpdatableStorage(fr8DataDTO.ActivityDTO))
            {
                crateStorage.Add(CreateConnectionStringCrate());
            }

            AddOperationalStateCrate(fr8DataDTO, new OperationalStateCM());

            AddPayloadCrate(fr8DataDTO,
               new StandardPayloadDataCM(
                    new KeyValueDTO("Field1", "[Customer].[Physician]"),
                    new KeyValueDTO("Field2", "[Customer].[CurrentMedicalCondition]")
               ),
               "MappedFields"
            );

            AddPayloadCrate(fr8DataDTO,
                new StandardPayloadDataCM(
                    new KeyValueDTO("Field1", "test physician"),
                    new KeyValueDTO("Field2", "teststring")
                ),
               "TableData"
            );

            var responsePayloadDTO = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, fr8DataDTO);

            Assert.NotNull(responsePayloadDTO);
            Assert.NotNull(responsePayloadDTO.CrateStorage);
            Assert.NotNull(responsePayloadDTO.CrateStorage.Crates);
        }

        [Test]
        public async Task Write_To_Sql_Server_Activate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalActivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var fr8DataDTO = HealthMonitor_FixtureData.Write_To_Sql_Server_v1_InitialConfiguration_Fr8DataDTO();

            //Act
            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    fr8DataDTO
                );

            //Assert
            Assert.IsNotNull(responseActionDTO);
            Assert.IsNotNull(Crate.FromDto(responseActionDTO.CrateStorage));
        }

        [Test]
        public async Task Write_To_Sql_Server_Deactivate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalDeactivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var fr8DataDTO = HealthMonitor_FixtureData.Write_To_Sql_Server_v1_InitialConfiguration_Fr8DataDTO();

            //Act
            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    fr8DataDTO
                );

            //Assert
            Assert.IsNotNull(responseActionDTO);
            Assert.IsNotNull(Crate.FromDto(responseActionDTO.CrateStorage));
        }
    }
}
