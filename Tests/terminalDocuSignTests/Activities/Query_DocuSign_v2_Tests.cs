using System;
using System.Threading.Tasks;
using Data.Entities;
using DocuSign.eSign.Model;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Hub.Managers;
using Moq;
using NUnit.Framework;
using StructureMap;
using terminalDocuSign.Actions;
using terminalDocuSign.Interfaces;
using terminalDocuSign.Services;
using terminalDocuSign.Services.New_Api;
using UtilitiesTesting.Fixtures;

namespace terminalDocuSignTests.Activities
{
    [TestFixture]
    public class Query_DocuSign_v2_Tests : BaseTest
    {
        [Test]
        public async Task Initialize_Always_FillsFolderAndStatusSources()
        {
            ObjectFactory.GetInstance<Mock<IDocuSignFolders>>().Setup(x => x.GetFolders(It.IsAny<DocuSignApiConfiguration>()))
                .Returns(new[] { new FieldDTO("Name", "Id") });
            var activity = ObjectFactory.Container.GetInstance<Query_DocuSign_v2>();
            var currentActivity = await activity.Configure(new ActivityDO(), FakeToken);
            var activityUi = currentActivity.GetReadonlyActivityUi<Query_DocuSign_v2.ActivityUi>();
            Assert.IsTrue(activityUi.FolderFilter.ListItems.Count > 0, "Folder filter was not filled with items");
            Assert.IsTrue(activityUi.StatusFilter.ListItems.Count > 0, "Status filter was not filled with items");
        }

        [Test]
        public async Task Initialize_Always_ReportsRuntimeCratesAndFields()
        {
            var activity = ObjectFactory.Container.GetInstance<Query_DocuSign_v2>();
            var currentActivity = await activity.Configure(new ActivityDO(), FakeToken);
            var crateDescriptionsManifest = CrateManager.GetStorage(currentActivity).FirstCrateOrDefault<CrateDescriptionCM>()?.Content;
            Assert.IsNotNull(crateDescriptionsManifest, "No runtime available crates were reported");
            Assert.IsTrue(crateDescriptionsManifest.CrateDescriptions.Count > 0, "No runtime available crates were reported");
            Assert.IsTrue(crateDescriptionsManifest.CrateDescriptions[0].Fields?.Count > 0, "No runtime available fields were reported");
        }

        [Test]
        public async Task Run_Always_ProducesExpectedCrate()
        {
            ObjectFactory.Container.ConfigureHubToReturnEmptyPayload();
            ObjectFactory.Container.GetInstance<Mock<IDocuSignFolders>>().Setup(x => x.GetFolderItems(It.IsAny<DocuSignApiConfiguration>(), It.IsAny<DocuSignQuery>()))
                         .Returns(new[] { new FolderItem() });
            var activity = ObjectFactory.Container.GetInstance<Query_DocuSign_v2>();
            var currentActivity = await activity.Configure(new ActivityDO(), FakeToken);
            var resultPayload = await activity.Run(currentActivity, Guid.Empty, FakeToken);
            var resultManifest = CrateManager.GetStorage(resultPayload).FirstCrateOrDefault<DocuSignEnvelopeCM_v3>()?.Content;
            Assert.IsNotNull(resultManifest, "Run didn't produce crate with expected manifest");
            Assert.AreEqual(1, resultManifest.Envelopes.Count, "Run didn't get expected number of envelopes");
        }
    }
}
