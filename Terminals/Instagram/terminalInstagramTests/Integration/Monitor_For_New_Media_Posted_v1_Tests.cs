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
using terminalInstagramTests.Fixtures;

namespace terminalInstagramTests.Integration
{
    [Explicit]
    public class Monitor_For_New_Media_Posted_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName => "terminalInstagram";

        private const string InstagramMediaId = "Media Id";
        private const string InstagramCaptionId = "Caption Id";
        private const string InstagramCaptionText = "Caption Text";
        private const string InstagramCaptionCreatedTimeField = "Caption Time";
        private const string InstagramImageUrl = "Image Url";
        private const string InstagramImageUrlStandardResolution = "Image Url Standard Resolution";

        private void AssertConfigureControls(StandardConfigurationControlsCM control)
        {
            Assert.AreEqual(1, control.Controls.Count, "Control count is not 1");
            Assert.IsTrue(control.Controls[0] is TextBlock, "First control isn't a TextBlock");
            Assert.AreEqual("Description", control.Controls[0].Label, "Invalid Label on control");
            Assert.AreEqual("Description", control.Controls[0].Name, "Invalid Name on control");
        }

        private void AssertConfigureCrate(ICrateStorage crateStorage)
        {
            Assert.AreEqual(3, crateStorage.Count, "Crate storage count is not equal to 3");
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(), "StandardConfigurationControlsCM count is not 1");
            Assert.AreEqual(1, crateStorage.CratesOfType<EventSubscriptionCM>().Count(), "EventSubscriptionCM count is not 1");
            Assert.AreEqual(1, crateStorage.CratesOfType<CrateDescriptionCM>().Count(), "FieldDescriptionsCM count is not 1");
            AssertConfigureControls(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());
            var fieldDescriptions = crateStorage.CratesOfType<CrateDescriptionCM>().Single();
            Assert.AreEqual("Runtime Available Crates", fieldDescriptions.Label, "Monitor Instagram Runtime Fields labeled FieldDescriptionsCM was not found");
            Assert.AreEqual(1, fieldDescriptions.Content.CrateDescriptions.Count(), "CrateDescriptions count is not 1");
            var fields = fieldDescriptions.Content.CrateDescriptions.Single().Fields;

            Assert.AreEqual("Monitor Instagram Runtime Fields", fieldDescriptions.Content.CrateDescriptions.Single().Label, "Monitor Instagram Runtime Fields labeled CrateDescription was not found");
            Assert.AreEqual(6, fieldDescriptions.Content.CrateDescriptions.Single().Fields.Count, "Published runtime field count is not 6");

            Assert.IsTrue(fields.Exists(x => x.Name == InstagramMediaId), "InstagramMediaId is not signalled");
            Assert.IsTrue(fields.Exists(x => x.Name == InstagramCaptionId), "InstagramCaptionId is not signalled");
            Assert.IsTrue(fields.Exists(x => x.Name == InstagramCaptionText), "InstagramCaptionText is not signalled");
            Assert.IsTrue(fields.Exists(x => x.Name == InstagramCaptionCreatedTimeField), "InstagramCaptionCreatedTimeField is not signalled");
            Assert.IsTrue(fields.Exists(x => x.Name == InstagramImageUrl), "InstagramImageUrl is not signalled");
            Assert.IsTrue(fields.Exists(x => x.Name == InstagramImageUrlStandardResolution), "InstagramImageUrlStandardResolution is not signalled");

        }

        private async Task<ActivityDTO> CompleteInitialConfiguration()
        {
            var configureUrl = GetTerminalConfigureUrl();
            var requestDataDTO = FixtureData.MonitorForNewMediaPosted_InitialConfiguration_Fr8DataDTO();
            return await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, requestDataDTO);
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
        public async Task MonitorForNewMediaPosted_Initial_Configuration_Check_Crate_Structure()
        {
            var responseDTO = await CompleteInitialConfiguration();
            AssertInitialConfigurationResponse(responseDTO);
        }


        /// <summary>
        /// Validate correct crate-storage structure in followup configuration response.
        /// </summary>
        [Test]
        public async Task MonitorForNewMediaPosted_Run_Without_EventPayload_Should_Terminate()
        {
            var configureUrl = GetTerminalConfigureUrl();
            var responseDTO = await CompleteInitialConfiguration();
            responseDTO.AuthToken = FixtureData.Instagram_AuthToken();
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