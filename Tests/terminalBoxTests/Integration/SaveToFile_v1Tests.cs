using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Testing.Integration;
using NUnit.Framework;
using terminalBoxTests.Fixtures;

namespace terminalBoxTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    [Category("Integration.terminalBox")]
    public class SaveToFile_v1Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName => "terminalBox";

        [Test]
        public async Task RunSavesFileToBox()
        {
            // Arrange
            var configureUrl = GetTerminalConfigureUrl();
            var dataDTO = FixtureData.SaveToFileTestAction();
            var runUrl = GetTerminalRunUrl();
            // Act
            var responseDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    dataDTO
                );

            dataDTO.ActivityDTO = responseDTO;

            using (var crateStorage = Crate.GetUpdatableStorage(dataDTO.ActivityDTO))
            {
                // Add upstream table data
                var tableCrate = FixtureData.GetStandardTableDataCM();
                AddPayloadCrate(dataDTO, tableCrate);

                // Select table data in CrateChooser
                var controls = crateStorage
                    .CrateContentsOfType<StandardConfigurationControlsCM>()
                    .Single();

                var fileChooser = controls.FindByName<CrateChooser>("FileChooser");
                fileChooser.CrateDescriptions = new List<CrateDescriptionDTO>()
                {
                    new CrateDescriptionDTO() {Label = "", Selected = true}
                };

                AddOperationalStateCrate(dataDTO, new OperationalStateCM());
                dataDTO.ActivityDTO.AuthToken = FixtureData.GetBoxAuthToken();
            }


            var payload = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(
                    runUrl,
                    dataDTO
                );
            // Assert
            Assert.NotNull(payload);
            Assert.NotNull(payload.CrateStorage);
        }
    }
}