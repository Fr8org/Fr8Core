using System.Threading.Tasks;
using Data.States;
using Fr8.Testing.Integration;
using NUnit.Framework;

namespace terminalFr8CoreTests.Integration
{
    [Explicit]
    public class Loop_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName => "terminalFr8Core";
        
        [Test]
        public async Task FollowUpConfig_ShouldUpdateLabel_WhenUpstreamCrateUpdates()
        {
            /*
            // Arrange
            var configureUrl = GetTerminalConfigureUrl();
            var dataDTO = FixtureData.Loop_InitialConfiguration_ActivityDTO();
            var standardTableDataCm = FixtureData.StandardTableData_Test1();
            AddUpstreamCrate(dataDTO, standardTableDataCm);
            // Act
            var firstResponseActionDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                   configureUrl,
                   dataDTO
               );
            dataDTO.ActivityDTO = firstResponseActionDTO;
            using (var crateStorage = Crate.GetUpdatableStorage(dataDTO.ActivityDTO))
            {
                var controls = crateStorage
                    .CrateContentsOfType<StandardConfigurationControlsCM>()
                    .Single();
                var crateChooser = controls.Controls.OfType<CrateChooser>().Single();
                crateChooser.CrateDescriptions.Add(new CrateDescriptionDTO()
                {
                    Label = "oldLabel",
                    Selected = true,
                    ManifestType = "Standard Table Data",
                    Availability = AvailabilityType.RunTime
                });
                crateStorage.Add(Fr8Data.Crates.Crate.FromContent("HealthMonitor_UpstreamCrate_newLabel", new StandardTableDataCM()));
            }

            var responseActionDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                 configureUrl,
                 dataDTO
             );
           
            // Assert
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            var chooser = Crate.GetStorage(responseActionDTO)
               .CrateContentsOfType<StandardConfigurationControlsCM>()
                   .Single().Controls.OfType<CrateChooser>().Single();
            Assert.AreEqual(
                "newLabel", 
                chooser.CrateDescriptions[0].Label);
                */
        }
    }
}
