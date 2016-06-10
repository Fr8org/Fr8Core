using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Testing.Integration;
using NUnit.Framework;
using terminalQuickBooksTests.Fixtures;

namespace terminalQuickBooksTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    internal class Create_Journal_Entry_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName => "terminalQuickBooks";

        [Test, Category("Integration.terminalQuickBooks")]
        public async Task Create_Journal_Entry_Configuration_Check_With_No_Upstream_Crate()
        {
            //Arrange
            var curMessage =
                "When this Action runs, it will be expecting to find a Crate of Standard Accounting Transactions. " +
                "Right now, it doesn't detect any Upstream Actions that produce that kind of Crate. " +
                "Please add an activity upstream (to the left) of this action that does so.";
            var configureUrl = GetTerminalConfigureUrl();
            var requestActionDTO = HealthMonitor_FixtureData.Activity_Create_Journal_Entry_v1_InitialConfiguration_Fr8DataDTO();
            //Act
            var responseActionDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                );
            //Assert
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            var controls = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single().Controls;
            Assert.AreEqual(0, controls.Count);
        }
    }
}
