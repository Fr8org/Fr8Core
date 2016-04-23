using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;
using terminalDocuSign.Activities;
using terminalDocuSign.Infrastructure;
using terminalDocuSign.Infrastructure.AutoMapper;
using terminalDocuSign.Infrastructure.StructureMap;
using terminalDocuSign.Interfaces;
using terminalDocuSign.Tests.Fixtures;
using TerminalBase.Infrastructure;
using Utilities.Configuration.Azure;
using UtilitiesTesting;

namespace terminalDocuSign.Tests.Actions
{
    [TestFixture]
    [Category("terminalDocuSign")]
    public class Search_DocuSign_Tests : BaseTest
    {
        Search_DocuSign_History_v1 _activity;
        private ICrateManager _crateManager;

        public override void SetUp()
        {
            base.SetUp();

            //TODO: rework
            //Commented out by Serget, FR-2400
            //var docusignFolder = new Mock<IDocuSignFolder>();

            //docusignFolder.Setup(r => r.GetSearchFolders(It.IsAny<string>(), It.IsAny<string>())).Returns(TerminalFixtureData.GetFolders());
            //docusignFolder.Setup(r => r.Search(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<IEnumerable<FilterConditionDTO>>()))
            //    .Returns<string, string, string, string, string, DateTime?, DateTime?>(Search);

            //TerminalBootstrapper.ConfigureTest();
            //TerminalDocuSignMapBootstrapper.ConfigureDependencies(Hub.StructureMap.StructureMapBootStrapper.DependencyType.TEST);
            //TerminalDataAutoMapperBootStrapper.ConfigureAutoMapper();
            //CloudConfigurationManager.RegisterApplicationSettings(new AppSettingsFixture());

            //ObjectFactory.Configure(cfg => cfg.For<IDocuSignFolder>().Use(docusignFolder.Object));


            //var hubCommunicator = new Mock<IHubCommunicator>();
            //hubCommunicator.Setup(r => r.GetPayload(It.IsAny<ActivityDO>(), It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(new PayloadDTO(Guid.Empty) { CrateStorage = new CrateStorageDTO() }));
            //hubCommunicator.Setup(r => r.GetActivityTemplates(It.IsAny<string>())).Returns(Task.FromResult(new List<ActivityTemplateDTO>()
            //{
            //    new ActivityTemplateDTO()
            //    {
            //        Name = "Query_DocuSign"
            //    }
            //}));

            //ObjectFactory.Configure(cfg => cfg.For<IHubCommunicator>().Use(hubCommunicator.Object));

            //_activity = new Search_DocuSign_History_v1();

            //_crateManager = ObjectFactory.GetInstance<ICrateManager>();
        }

        private static List<FolderItem> Search(string login, string password, string searchText, string folderId, string status = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            return TerminalFixtureData.GetFolderInfo(folderId);
        }
        
        [Test]
        public async Task Can_Fill_List_Of_Folders()
        {
            //TODO: Rework
            //FR-2400
            //var curAuthTokenDO = Mapper.Map<AuthorizationTokenDO>(new AuthorizationTokenDTO() {Token = JsonConvert.SerializeObject(TerminalFixtureData.TestDocuSignAuthDTO1())});

            //var result = await _activity.Configure(new ActivityDO(), curAuthTokenDO);
            //var storage = _crateManager.GetStorage(result);

            //var foldersCrate = storage.CratesOfType<FieldDescriptionsCM>().Where(x => x.Label == "Folders").Select(x => x.Content).FirstOrDefault();
            //Assert.IsNotNull(foldersCrate);

            //foreach (var f in TerminalFixtureData.GetFolders())
            //{
            //    var local = f;
            //    Assert.AreEqual(foldersCrate.Fields.Count(x => x.Key == local.Name && x.Value == local.FolderId), 1);
            //}
        }

        [Test]
        public async Task Can_Configure_Children_Activities()
        {
            //TODO: Rework
            //FR-2400

            //var curAuthTokenDO = Mapper.Map<AuthorizationTokenDO>(new AuthorizationTokenDTO() { Token = JsonConvert.SerializeObject(TerminalFixtureData.TestDocuSignAuthDTO1()) });

            //var activity = new ActivityDO();

            //using (var crateStorage = _crateManager.GetUpdatableStorage(activity))
            //{
            //    crateStorage.Add(Crate.FromContent("UI", new Search_DocuSign_History_v1.ActivityUi
            //    {
            //        Folder = { Value = "A"},
            //        SearchText = { Value = "B"},
            //        Status = { Value = "C"}
            //    }));
            //}

            //var result = await _activity.Configure(activity, curAuthTokenDO);

            //Assert.AreEqual(1, result.ChildNodes.Count);
            //Assert.AreEqual("Query_DocuSign", ((ActivityDO) result.ChildNodes[0]).ActivityTemplate.Name);


            //var storage = _crateManager.GetStorage(((ActivityDO) result.ChildNodes[0]).CrateStorage);
            //var configurationControls = storage.CrateContentsOfType<StandardConfigurationControlsCM>().First();
            //var actionUi = new Query_DocuSign_v1.ActivityUi();

            //actionUi.ClonePropertiesFrom(configurationControls);

            //Assert.AreEqual("A", actionUi.Folder.Value);
            //Assert.AreEqual("B", actionUi.SearchText.Value);
            //Assert.AreEqual("C", actionUi.Status.Value);
        }
    }
}