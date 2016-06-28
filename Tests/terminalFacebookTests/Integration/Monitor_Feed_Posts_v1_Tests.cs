using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Testing.Integration;
using NUnit.Framework;
using terminalFacebookTests.Fixtures;

namespace terminalFacebookTests.Integration
{
    [Explicit]
    public class Monitor_Feed_Posts_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName => "terminalFacebook";

        private const string FacebookFeedIdField = "Feed Id";
        private const string FacebookFeedMessageField = "Feed Message";
        private const string FacebookFeedStoryField = "Feed Story";
        private const string FacebookFeedCreatedTimeField = "Feed Time";

        private void AssertConfigureControls(StandardConfigurationControlsCM control)
        {
            Assert.AreEqual(1, control.Controls.Count, "Control count is not 1");
            Assert.IsTrue(control.Controls[0] is TextBlock, "First control isn't a TextBlock");
            Assert.AreEqual("Message", control.Controls[0].Label, "Invalid Label on control");
            Assert.AreEqual("Message", control.Controls[0].Name, "Invalid Name on control");
        }

        private void AssertConfigureCrate(ICrateStorage crateStorage)
        {
            Assert.AreEqual(3, crateStorage.Count, "Crate storage count is not equal to 3");
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(), "StandardConfigurationControlsCM count is not 1");
            Assert.AreEqual(1, crateStorage.CratesOfType<EventSubscriptionCM>().Count(), "EventSubscriptionCM count is not 1");
            Assert.AreEqual(1, crateStorage.CratesOfType<CrateDescriptionCM>().Count(), "FieldDescriptionsCM count is not 1");
            AssertConfigureControls(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());
            var fieldDescriptions = crateStorage.CratesOfType<CrateDescriptionCM>().Single();
            Assert.AreEqual("Runtime Available Crates", fieldDescriptions.Label, "Monitor Facebook Runtime Fields labeled FieldDescriptionsCM was not found");
            Assert.AreEqual(1, fieldDescriptions.Content.CrateDescriptions.Count(), "CrateDescriptions count is not 1");
            var fields = fieldDescriptions.Content.CrateDescriptions.Single().Fields;
            
            Assert.AreEqual("Monitor Facebook Runtime Fields", fieldDescriptions.Content.CrateDescriptions.Single().Label, "Monitor Facebook Runtime Fields labeled CrateDescription was not found");
            Assert.AreEqual(4, fieldDescriptions.Content.CrateDescriptions.Single().Fields.Count, "Published runtime field count is not 4");
            Assert.IsTrue(fields.Exists(x => x.Name == FacebookFeedIdField), "FacebookFeedIdField is not signalled");
            Assert.IsTrue(fields.Exists(x => x.Name == FacebookFeedMessageField), "FacebookFeedMessageField is not signalled");
            Assert.IsTrue(fields.Exists(x => x.Name == FacebookFeedStoryField), "FacebookFeedStoryField is not signalled");
            Assert.IsTrue(fields.Exists(x => x.Name == FacebookFeedCreatedTimeField), "FacebookFeedCreatedTimeField is not signalled");

        }

        private async Task<ActivityDTO> CompleteInitialConfiguration()
        {
            var configureUrl = GetTerminalConfigureUrl();
            var requestDataDTO = FixtureData.MonitorFeedPosts_InitialConfiguration_Fr8DataDTO();
            return await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl,requestDataDTO);
        }

        private void AssertInitialConfigurationResponse(ActivityDTO responseDTO)
        {
            Assert.NotNull(responseDTO, "Response is null on initial configuration");
            Assert.NotNull(responseDTO.CrateStorage, "Crate storage is null on initial configuration");
            var crateStorage = Crate.FromDto(responseDTO.CrateStorage);
            AssertConfigureCrate(crateStorage);
        }

        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test]
        public async Task MonitorFeedPosts_Initial_Configuration_Check_Crate_Structure()
        {
            var responseDTO = await CompleteInitialConfiguration();
            AssertInitialConfigurationResponse(responseDTO);
        }


        /// <summary>
        /// Validate correct crate-storage structure in followup configuration response.
        /// </summary>
        [Test]
        public async Task MonitorFeedPosts_Run_Without_EventPayload_Should_Terminate()
        {
            var configureUrl = GetTerminalConfigureUrl();
            var responseDTO = await CompleteInitialConfiguration();
            responseDTO.AuthToken = FixtureData.Facebook_AuthToken();
            var dataDTO = new Fr8DataDTO
            {
                ActivityDTO = responseDTO
            };

            var runUrl = GetTerminalRunUrl();

            AddOperationalStateCrate(dataDTO, new OperationalStateCM());
            //nothing should change on followup
            var responsePayload = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);

            var crateStorage = Crate.GetStorage(responsePayload);
            var operationalStateCM = crateStorage.CrateContentsOfType<OperationalStateCM>().Single();
            Assert.AreEqual(ActivityResponse.RequestTerminate.ToString(), operationalStateCM.CurrentActivityResponse.Type, "Activity response is not terminate");
        }

    }
}