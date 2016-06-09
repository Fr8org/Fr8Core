using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Testing.Integration;
using NUnit.Framework;
using terminalSlackTests.Fixtures;

namespace terminalSlackTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    [Category("terminalSlack.Integration")]
    public class Publish_To_Slack_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName => "terminalSlack";

        [Test]
        public async Task Publish_To_Slack_v1_ProcessConfigurationRequest()
        {
            // Act
            var responseFollowUpActionDTO = await ConfigurationRequest();

            // Assert
            Assert.NotNull(responseFollowUpActionDTO);
        }

        private async Task<ActivityDTO> ConfigurationRequest()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestDataDTO = HealthMonitor_FixtureData.Publish_To_Slack_v1_InitialConfiguration_Fr8DataDTO();

            var responseActionDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, requestDataDTO);

            var storage = Crate.GetStorage(responseActionDTO);

            using (var crateStorage = Crate.GetUpdatableStorage(requestDataDTO.ActivityDTO))
            {
                crateStorage.Replace(storage);
            }

            return await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, requestDataDTO);
        }

        [Test]
        public async Task Publish_To_Slack_v1_Initial_Configuration_Check_Crate_Structure_NoAuth()
        {
            // Act
            var responseActionDTO = await ConfigureInitial(false);
            var crateStorage = Crate.GetStorage(responseActionDTO);
            Assert.AreEqual(1, crateStorage.Count, "Configuration response for not-autheticated call should contain only one crate");
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardAuthenticationCM>().Count(), "Configuration response for non-autheticated call doesn't contain Standard Authentication crate" );
        }

        [Test]
        public async Task Publish_To_Slack_v1_Initial_Configuration_Check_Crate_Structure()
        {
            // Act
            var responseActionDTO = await ConfigureInitial();

            // Assert
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);

            Assert.AreEqual(1, crateStorage.Count, "Configuration response for autheticated call should contain only one crate");
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(), "Configuration response for autheticated call doesn't contain Standard Configuration Controls crate");
        }

        private async Task<ActivityDTO> ConfigureInitial(bool isAuthToken = true)
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Publish_To_Slack_v1_InitialConfiguration_Fr8DataDTO(isAuthToken);
            var responseActionDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, requestActionDTO);

            return responseActionDTO;
        }
    }
}
