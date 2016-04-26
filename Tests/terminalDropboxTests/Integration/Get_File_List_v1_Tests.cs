using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using Data.Crates;
using Hub.Managers;
using Microsoft.Owin.Hosting;
using Ploeh.AutoFixture;
using terminalDropbox;
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

        private const string Host = "http://localhost:19760";
        private IDisposable _app;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            try
            {
                _app = WebApp.Start<Startup>(Host);
            }
            catch
            {
                /* Ignored
                We need this empty exception handling when terminal already started.
                So, if you already start terminal manually (or it started on build server),
                there is no need to use self-hosted Owin server
                */
            }
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            _app?.Dispose();
        }

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
            Assert.NotNull(responseActionDTO.CrateStorage.Crates);
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
                crateStorage.Add(Crate.CreateStandardConfigurationControlsCrate("Configuration_Controls"));
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
                crateStorage.Add(Crate.CreateStandardConfigurationControlsCrate("Configuration_Controls"));
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
