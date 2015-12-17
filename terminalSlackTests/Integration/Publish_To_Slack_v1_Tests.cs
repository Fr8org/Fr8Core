using System.Linq;
using System.Threading.Tasks;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Hub.StructureMap;
using NUnit.Framework;
using StructureMap;
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
    public class Publish_To_Slack_v1_Tests : BaseHealthMonitorTest
    {
        public override string TerminalName
        {
            get { return "terminalSlack"; }
        }

        [Test]
        public async void Publish_To_Slack_v1_ProcessConfigurationRequest()
        {
            // Act
            var responseFollowUpActionDTO = await ConfigurationRequest();

            // Assert
            Assert.NotNull(responseFollowUpActionDTO);
        }

        private async Task<ActionDTO> ConfigurationRequest()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Publish_To_Slack_v1_InitialConfiguration_ActionDTO();

            var responseActionDTO = await HttpPostAsync<ActionDTO, ActionDTO>(configureUrl, requestActionDTO);

            var storage = Crate.GetStorage(responseActionDTO);

            using (var updater = Crate.UpdateStorage(requestActionDTO))
            {
                updater.CrateStorage = storage;
            }

            return await HttpPostAsync<ActionDTO, ActionDTO>(configureUrl, requestActionDTO);
        }

        [Test]
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException),
            ExpectedMessage = @"{""status"":""terminal_error"",""message"":""One or more errors occurred.""}"
        )]
        public async void Publish_To_Slack_v1_Initial_Configuration_Check_Crate_Structure_NoAuth()
        {
            // Act
            var responseActionDTO = await ConfigureInitial(false);
        }

        [Test]
        public async void Publish_To_Slack_v1_Initial_Configuration_Check_Crate_Structure()
        {
            // Act
            var responseActionDTO = await ConfigureInitial();

            // Assert
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            Assert.NotNull(responseActionDTO.CrateStorage.Crates);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            AssertCrateTypes(crateStorage);
        }

        private async Task<ActionDTO> ConfigureInitial(bool isAuthToken = true)
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Publish_To_Slack_v1_InitialConfiguration_ActionDTO(isAuthToken);
            var responseActionDTO = await HttpPostAsync<ActionDTO, ActionDTO>(configureUrl, requestActionDTO);

            return responseActionDTO;
        }

        private void AssertCrateTypes(CrateStorage crateStorage)
        {
            Assert.AreEqual(4, crateStorage.Count);

            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(x => x.Label == "Configuration_Controls"));
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardDesignTimeFieldsCM>().Count(x => x.Label == "Available Fields"));
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardDesignTimeFieldsCM>().Count(x => x.Label == "Available Channels"));
            Assert.AreEqual(1, crateStorage.CratesOfType<EventSubscriptionCM>().Count(x => x.Label == "Standard Event Subscriptions"));
        }
    }
}
