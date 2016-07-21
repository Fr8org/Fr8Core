using System.Linq;
using Fr8.Testing.Integration;
using NUnit.Framework;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using terminalDropboxTests.Fixtures;

namespace terminalDropboxTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    [Category("Integration.terminalDropbox")]
    public class Get_File_List_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName => "terminalDropbox";

        [Test]
        public async Task GetFileList_InitialConfig_ReturnsActivity()
        {
            //Arrange
            var configureUrl = GetTerminalConfigureUrl();
            Fr8DataDTO requestActionDTO = HealthMonitor_FixtureData.GetFileListTestFr8DataDTO();

            //Act
            var responseActionDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                    );

            // Assert
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
        }

        [Test]
        public async Task Activate_Returns_ActivityDTO()
        {
            //Arrange
            var activateUrl = GetTerminalActivateUrl();
            Fr8DataDTO dataDto = HealthMonitor_FixtureData.GetFileListTestFr8DataDTO();

            // Add initial configuretion controls
            using (var crateStorage = Crate.GetUpdatableStorage(dataDto.ActivityDTO))
            {
                crateStorage.Add("Configuration_Controls", new StandardConfigurationControlsCM());
            }

            //Act
            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    activateUrl,
                    dataDto
                );

            //Assert
            Assert.IsNotNull(responseActionDTO);
            Assert.IsNotNull(Crate.FromDto(responseActionDTO.CrateStorage));
        }

        [Test]
        public async Task Run_Returns_ActivityDTO()
        {
            //Arrange
            var runUrl = GetTerminalRunUrl();
            Fr8DataDTO dataDto = HealthMonitor_FixtureData.GetFileListTestFr8DataDTO();

            // Add initial configuretion controls
            using (var crateStorage = Crate.GetUpdatableStorage(dataDto.ActivityDTO))
            {
                crateStorage.Add("Configuration_Controls", new StandardConfigurationControlsCM());
            }
            // Add operational state crate
            AddOperationalStateCrate(dataDto, new OperationalStateCM());

            //Act
            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, PayloadDTO>(
                    runUrl,
                    dataDto
                );

            //Assert
            Assert.IsNotNull(responseActionDTO);
            var crateFromDTO = Crate.FromDto(responseActionDTO.CrateStorage);
            Assert.IsNotNull(crateFromDTO);
            Assert.Greater(crateFromDTO.CratesOfType<StandardFileListCM>().Count(), 0);
        }
    }
}
