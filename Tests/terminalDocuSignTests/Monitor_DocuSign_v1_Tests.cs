using System.Configuration;
using NUnit.Framework;
using Data.Interfaces.DataTransferObjects;
using HealthMonitor.Utility;

namespace terminalDocuSignTests
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    public class Monitor_DocuSign_v1_Tests : BaseHealthMonitorTest
    {
        public override string TerminalName
        {
            get { return "terminalDocuSign"; }
        }

        [Test]
        public async void Example_Initial_Configuration_Check_Crate_Types()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Monitor_DocuSign_v1_InitialConfiguration_ActionDTO();

            var responseActionDTO = await JsonRestClient
                .PostAsync<ActionDTO, ActionDTO>(
                    configureUrl,
                    requestActionDTO
                );

            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            Assert.NotNull(responseActionDTO.CrateStorage.Crates);
            Assert.AreEqual(responseActionDTO.CrateStorage.Crates.Length, 4);
        }
    }
}
