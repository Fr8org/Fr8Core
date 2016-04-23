using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;
using terminalDocuSign.Activities;
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
    public class Query_DocuSign_Tests : BaseTest
    {
        Query_DocuSign_v1 _activity;
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

            //PayloadDTO payloadDto = new PayloadDTO(Guid.Empty);
            //payloadDto.CrateStorage = new CrateStorageDTO();
            //using (var crateStorage = new CrateManager().GetUpdatableStorage(payloadDto))
            //{
            //    var operationalStatus = new OperationalStateCM();
            //    var operationsCrate = Crate.FromContent("Operational Status", operationalStatus);
            //    crateStorage.Add(operationsCrate);
            //}


            //var restfulServiceClient = new Mock<IRestfulServiceClient>();
            //restfulServiceClient.Setup(r => r.GetAsync<PayloadDTO>(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
            //    .Returns(Task.FromResult(payloadDto));
            //ObjectFactory.Configure(cfg => cfg.For<IRestfulServiceClient>().Use(restfulServiceClient.Object));

            //_activity = new Query_DocuSign_v1();

            //_crateManager = ObjectFactory.GetInstance<ICrateManager>();
        }

        private static List<FolderItem> Search(string login, string password, string searchText, string folderId, string status = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            return TerminalFixtureData.GetFolderInfo(folderId);
        }

        private static bool CheckRow(FolderItem item, PayloadObjectDTO obj)
        {
            return item.Name == obj.GetValue("Name") &&
                   item.EnvelopeId == obj.GetValue("Id") &&
                   item.Subject == obj.GetValue("Subject") &&
                   item.Status == obj.GetValue("Status") &&
                   item.OwnerName == obj.GetValue("OwnerName") &&
                   item.SenderName == obj.GetValue("SenderName") &&
                   item.SenderEmail == obj.GetValue("SenderEmail") &&
                   item.Shared == obj.GetValue("Shared") &&
                   item.CompletedDateTime.ToString(CultureInfo.InvariantCulture) == obj.GetValue("CompletedDate") &&
                   item.CreatedDateTime.ToString(CultureInfo.InvariantCulture) == obj.GetValue("CreatedDateTime");
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
        public async Task Can_Search_By_One_Folder()
        {
            //TODO: Rework
            //FR-2400

            //var curAuthTokenDO = Mapper.Map<AuthorizationTokenDO>(new AuthorizationTokenDTO() { Token = JsonConvert.SerializeObject(TerminalFixtureData.TestDocuSignAuthDTO1()) });

            //var activity = new ActivityDO();
            
            //using (var crateStorage = _crateManager.GetUpdatableStorage(activity))
            //{
            //    crateStorage.Add(Crate.FromContent("Config", new Query_DocuSign_v1.ActivityUi
            //    {
            //        Folder = {Value = "folder_1"}
            //    }));
            //}

            //var result = await _activity.Run(activity, Guid.NewGuid(), curAuthTokenDO);
            //var storage = _crateManager.GetStorage(result);

            //var payload = storage.CrateContentsOfType<StandardPayloadDataCM>().FirstOrDefault();
            //Assert.IsNotNull(payload);
            
            //var referenceData = TerminalFixtureData.GetFolderInfo("folder_1");

            //Assert.AreEqual(referenceData.Count, payload.PayloadObjects.Count);
            
            //for (int i = 0; i < payload.PayloadObjects.Count; i ++)
            //{
            //    Assert.IsTrue(referenceData.Any(x => CheckRow(x, payload.PayloadObjects[i])));
            //}
        }

        [Test]
        public async Task Can_Search_By_Multiple_Folders()
        {
            //TODO: Rework
            //FR-2400

            //var curAuthTokenDO = Mapper.Map<AuthorizationTokenDO>(new AuthorizationTokenDTO() { Token = JsonConvert.SerializeObject(TerminalFixtureData.TestDocuSignAuthDTO1()) });

            //var activity = new ActivityDO();

            //using (var crateStorage = _crateManager.GetUpdatableStorage(activity))
            //{
            //    crateStorage.Add(Crate.FromContent("Config", new Query_DocuSign_v1.ActivityUi
            //    {
            //        Folder = {Value = "<any>"}
            //    }));
            //}

            //var result = await _activity.Run(activity, Guid.NewGuid(), curAuthTokenDO);
            //var storage = _crateManager.GetStorage(result);

            //var payload = storage.CrateContentsOfType<StandardPayloadDataCM>().FirstOrDefault();
            //Assert.IsNotNull(payload);

            //var referenceData = TerminalFixtureData.GetFolders().SelectMany(x=>TerminalFixtureData.GetFolderInfo(x.FolderId)).ToArray();

            //Assert.AreEqual(referenceData.Length, payload.PayloadObjects.Count);

            //for (int i = 0; i < payload.PayloadObjects.Count; i++)
            //{
            //    Assert.IsTrue(referenceData.Any(x => CheckRow(x, payload.PayloadObjects[i])));
            //}
        }

    }
}