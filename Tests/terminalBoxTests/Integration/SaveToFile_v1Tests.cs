using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;
using HealthMonitor.Utility;
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
            var requestActionDTO = FixtureData.SaveToFileTestAction();
            var runUrl = GetTerminalRunUrl();
            var tableCrate = FixtureData.GetStandardTableDataCM();
            AddUpstreamCrate(requestActionDTO, tableCrate);
            
            // Act
            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                );

            var payload = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(
                    configureUrl,
                    requestActionDTO
                );
            // Assert
            Assert.NotNull(payload);
            Assert.NotNull(payload.CrateStorage);
        }
    }
}