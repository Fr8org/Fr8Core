using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Interfaces;
using Fr8.TerminalBase.Infrastructure;
using Fr8.Testing.Unit;
using Moq;
using NUnit.Framework;
using StructureMap;
using terminalDocuSign.Infrastructure;
using terminalDocuSign.Infrastructure.StructureMap;
using terminalDocuSign.Tests.Fixtures;

namespace terminalDocuSign.Tests.Actions
{
    [TestFixture]
    [Category("terminalDocuSign")]
    public class Generate_DocuSign_Report_Tests : BaseTest
    {
        private ICrateManager _crateManager;

        public override void SetUp()
        {
            base.SetUp();
            
            TerminalBootstrapper.ConfigureTest();
            TerminalDocuSignMapBootstrapper.ConfigureDependencies(Hub.StructureMap.StructureMapBootStrapper.DependencyType.TEST);
            
            PayloadDTO payloadDto = new PayloadDTO(Guid.Empty);
            payloadDto.CrateStorage = new CrateStorageDTO();
            using (var crateStorage = new CrateManager().GetUpdatableStorage(payloadDto))
            {
                var operationalStatus = new OperationalStateCM();
                var operationsCrate = Crate.FromContent("Operational Status", operationalStatus);
                crateStorage.Add(operationsCrate);
            }


            var restfulServiceClient = new Mock<IRestfulServiceClient>();
            restfulServiceClient.Setup(r => r.GetAsync<PayloadDTO>(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .Returns(Task.FromResult(payloadDto));
            ObjectFactory.Configure(cfg => cfg.For<IRestfulServiceClient>().Use(restfulServiceClient.Object));

            
            
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
        }

        private static List<FolderItem> SearchDuplicates(string login, string password, string searchText, string folderId, string status = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            return TerminalFixtureData.GetFolderItems(folderId, 0, 20, "Sent");
        }

        private static List<FolderItem> SearchByStatus(string login, string password, string searchText, string folderId, string status = null, DateTime? fromDate = null, DateTime? toDate = null)
        {

            return TerminalFixtureData.GetFolderItems(folderId, 0, 10, "Sent")
                .Concat(TerminalFixtureData.GetFolderItems(folderId, 10, 10, "Created"))
                .Concat(TerminalFixtureData.GetFolderItems(folderId, 20, 10, "Signed"))
                .Where(x => x.Status == status)
                .ToList();
        }

        private static bool CheckRow(FolderItem item, PayloadObjectDTO obj)
        {
            return //item.Name == obj.GetValue("Name") &&
                   item.EnvelopeId == obj.GetValue("Id") &&
                   //item.Subject == obj.GetValue("Subject") &&
                   item.Status == obj.GetValue("Status") &&
                   //item.OwnerName == obj.GetValue("OwnerName") &&
                   //item.SenderName == obj.GetValue("SenderName") &&
                   //item.SenderEmail == obj.GetValue("SenderEmail") &&
                   //item.Shared == obj.GetValue("Shared") &&
                   item.CompletedDateTime.ToString(CultureInfo.InvariantCulture) == obj.GetValue("CompletedDate") &&
                   item.CreatedDateTime.ToString(CultureInfo.InvariantCulture) == obj.GetValue("CreatedDateTime");
        }
        
        public static DocuSignEnvelopeCM FolderItemToDocuSignEnvelopeCm(FolderItem folderItem)
        {
            return new DocuSignEnvelopeCM
            {
                EnvelopeId = folderItem.EnvelopeId,
                Status = folderItem.Status,
                CompletedDate = folderItem.CompletedDateTime,
                CreateDate = folderItem.CreatedDateTime
            };
        }


        [Test]
        public async Task Can_Search()
        {
            //TODO: rework
            //Commented out by Serget, FR-2400

            //var docusignFolder = new Mock<IDocuSignFolder>();

            //docusignFolder.Setup(r => r.GetSearchFolders(It.IsAny<string>(), It.IsAny<string>())).Returns(TerminalFixtureData.GetFolders());
            //docusignFolder.Setup(r => r.Search(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<IEnumerable<FilterConditionDTO>>()))
            //    .Returns<string, string, string, string, string, DateTime?, DateTime?>(SearchByStatus);

            //using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            //{
            //    foreach (var envelope in TerminalFixtureData.GetFolderItems("folder_1", 40, 10, "Sent").Select(FolderItemToDocuSignEnvelopeCm))
            //    {
            //        uow.MultiTenantObjectRepository.Add(envelope, "account");
            //    }

            //    foreach (var envelope in TerminalFixtureData.GetFolderItems("folder_1", 50, 10, "Created").Select(FolderItemToDocuSignEnvelopeCm))
            //    {
            //        uow.MultiTenantObjectRepository.Add(envelope, "account");
            //    }
            //}

            //ObjectFactory.Configure(cfg => cfg.For<IDocuSignFolder>().Use(docusignFolder.Object));

            //var curAuthTokenDO = Mapper.Map<AuthorizationTokenDO>(new AuthorizationTokenDTO { Token = JsonConvert.SerializeObject(TerminalFixtureData.TestDocuSignAuthDTO1()) });

            //var actionDo = new ActivityDO();

            //ConfigureActivity(actionDo, new KeyValuePair<string, string>("Status", "Sent"),
            //                          new KeyValuePair<string, string>("Folder", "folder_1"));

            //var activity = new Generate_DocuSign_Report_v1();
            //var result = await activity.Run(actionDo, Guid.NewGuid(), curAuthTokenDO);
            //var storage = _crateManager.GetStorage(result);

            //var payload = storage.CrateContentsOfType<StandardPayloadDataCM>().FirstOrDefault();

            //Assert.IsNotNull(payload);

            //var referenceData = TerminalFixtureData.GetFolderItems("folder_1", 40, 10, "Sent")
            //    .Concat(TerminalFixtureData.GetFolderItems("folder_1", 0, 10, "Sent")).ToList();

            //Assert.AreEqual(referenceData.Count, payload.PayloadObjects.Count);

            //for (int i = 0; i < payload.PayloadObjects.Count; i++)
            //{
            //    Assert.IsTrue(referenceData.Any(x => CheckRow(x, payload.PayloadObjects[i])));
            //}
        }

        [Test]
        public async Task Can_Eliminate_Duplicates()
        {
            //TODO: rework
            //Commented out by Serget, FR-2400

            //var docusignFolder = new Mock<IDocuSignFolder>();

            //docusignFolder.Setup(r => r.GetSearchFolders(It.IsAny<string>(), It.IsAny<string>())).Returns(TerminalFixtureData.GetFolders());
            //docusignFolder.Setup(r => r.Search(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<IEnumerable<FilterConditionDTO>>()))
            //    .Returns<string, string, string, string, string, DateTime?, DateTime?>(SearchDuplicates);

            //using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            //{
            //    foreach (var envelope in TerminalFixtureData.GetFolderItems("folder_1", 0, 30, "Sent").Select(FolderItemToDocuSignEnvelopeCm))
            //    {
            //        uow.MultiTenantObjectRepository.Add(envelope, "account");
            //    }
            //}

            //ObjectFactory.Configure(cfg => cfg.For<IDocuSignFolder>().Use(docusignFolder.Object));

            //var curAuthTokenDO = Mapper.Map<AuthorizationTokenDO>(new AuthorizationTokenDTO { Token = JsonConvert.SerializeObject(TerminalFixtureData.TestDocuSignAuthDTO1()) });

            //var actionDo = new ActivityDO();

            //ConfigureActivity(actionDo, new KeyValuePair<string, string>("Folder", "folder_1"));

            //var activity = new Generate_DocuSign_Report_v1();
            //var result = await activity.Run(actionDo, Guid.NewGuid(), curAuthTokenDO);
            //var storage = _crateManager.GetStorage(result);

            //var payload = storage.CrateContentsOfType<StandardPayloadDataCM>().FirstOrDefault();
          
            //Assert.IsNotNull(payload);

            //var referenceData = TerminalFixtureData.GetFolderItems("folder_1", 0, 30, "Sent");

            //Assert.AreEqual(referenceData.Count, payload.PayloadObjects.Count);
            
            //for (int i = 0; i < payload.PayloadObjects.Count; i ++)
            //{
            //    Assert.IsTrue(referenceData.Any(x => CheckRow(x, payload.PayloadObjects[i])));
            //}
        }
    }
}