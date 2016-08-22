using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;
using System.Linq;
using System.Web.Http.Results;

using Hub.Managers;
using Fr8.Testing.Unit;
using Fr8.Testing.Unit.Fixtures;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;

using HubWeb.Controllers;

namespace HubTests.Controllers
{
    [TestFixture]
    [Category("ManifestControllerTest")]
    public class ManifestControllerTest : BaseTest
    {
        private Fr8AccountDO _testUserAccount;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _testUserAccount = FixtureData.TestUser1();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.UserRepository.Add(_testUserAccount);
                uow.SaveChanges();

                ObjectFactory.GetInstance<ISecurityServices>().Login(uow, _testUserAccount);
            }
        }

        [TearDown]
        public void TearDown()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curUser = uow.UserRepository.GetQuery()
                    .SingleOrDefault(x => x.Id == _testUserAccount.Id);

                ObjectFactory.GetInstance<ISecurityServices>().Logout();

                uow.UserRepository.Remove(curUser);
                uow.SaveChanges();
            }
        }




        [Test]
        public void Check_StandardFr8PlansCM()
        {
            //Arrange
            var manifestController = CreateManifestController();            

            //Act
            int id = 19;
            var actionResult = manifestController.Get(id) as OkNegotiatedContentResult<CrateDTO>;

            var fieldsList = Deserialize(actionResult);
            ////Assert
            Assert.NotNull(fieldsList);
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("CreateDate")), "CreateDate Not found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("Description")), "Description Not found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("LastUpdated")), "LastUpdated Not found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("Name")), "Name Not found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("Ordering")), "Ordering Not found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("PlanState")), "PlanState Not found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("SubPlans")), "SubPlans Not found");
        }

        [Test]
        public void Check_StandardFr8HubsCM()
        {
            //Arrange
            var manifestController = CreateManifestController();

            //Act
            int id = 20;
            var actionResult = manifestController.Get(id) as OkNegotiatedContentResult<CrateDTO>;

            var fieldsList = Deserialize(actionResult);
            ////Assert
            Assert.NotNull(fieldsList);
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("CreatedDate")) ,"CreatedDate Not found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("Description")), "Description Not found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("LastUpdated")), "LastUpdated Not found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("Name")), "Name Not found");
        }

        [Test]
        public void Check_StandardFr8ContainersCM()
        {
            //Arrange
            var manifestController = CreateManifestController();

            //Act
            int id = 21;
            var actionResult = manifestController.Get(id) as OkNegotiatedContentResult<CrateDTO>;

            var fieldsList = Deserialize(actionResult);
            ////Assert
            Assert.NotNull(fieldsList);
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("CreatedDate")), "CreatedDate Not found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("Description")), "Description Not found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("LastUpdated")), "LastUpdated Not found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("Name")), "Name Not found");
        }
       
        [Test]
        public void Check_StandardParsingRecordCM()
        {
            //Arrange
            var manifestController = CreateManifestController();

            //Act
            int id = 22;
            var actionResult = manifestController.Get(id) as OkNegotiatedContentResult<CrateDTO>;
            var fieldsList = Deserialize(actionResult);
            ////Assert
            Assert.NotNull(fieldsList);
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("StartDate")), "StartDate Not Found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("Service")), "Service Not Found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("EndDate")), "EndDate Not Found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("Name")), "Name Not Found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("ExternalAccountId")), "ExternalAccountId Not Found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("InternalAccountId")), "InternalAccountid Not Found");
        }

        
        [Test]
        public void Check_StandardFr8TerminalCM()
        {
            //Arrange
            var manifestController = CreateManifestController();

            //Act
            int id = 23;
            var actionResult = manifestController.Get(id) as OkNegotiatedContentResult<CrateDTO>;
            var fieldsList = Deserialize(actionResult);
            ////Assert
            Assert.NotNull(fieldsList);
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("Definition")), "Definition Not Found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("Activities")), "Actions Not Found");          
        }
        
        //QueryDTO	List
        [Test]
        public void Check_StandardQueryCM()
        {
            //Arrange
            var manifestController = CreateManifestController();

            //Act
            int id = 17;
            var actionResult = manifestController.Get(id) as OkNegotiatedContentResult<CrateDTO>;
            var fieldsList = Deserialize(actionResult);
            ////Assert
            Assert.NotNull(fieldsList);
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("Queries")), "Queries Not Found");
        }


        [Test]
        public void Check_StandardEmailMessageCM()
        {
            //Arrange
            var manifestController = CreateManifestController();

            //Act
            int id = 18;
            var actionResult = manifestController.Get(id) as OkNegotiatedContentResult<CrateDTO>;
            var fieldsList = Deserialize(actionResult);
            ////Assert
            Assert.NotNull(fieldsList);
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("MessageID")), "MessageID Not Found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("Subject")), "Subject Not Found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("HtmlText")), "HtmlText Not Found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("PlainText")), "PlainText Not Found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("DateReceived")), "DateReceived Not Found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("EmailFromName")), "EmailFromName Not Found");
        }

        
        [Test]
        public void Check_StandardLoggingCM()
        {
            //Arrange
            var manifestController = CreateManifestController();

            //Act
            int id = 13;
            var actionResult = manifestController.Get(id) as OkNegotiatedContentResult<CrateDTO>;
            var fieldsList = Deserialize(actionResult);
            ////Assert
            Assert.NotNull(fieldsList);
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("Item")), "Item Not Found");
        }      

        [Test]
        public void Check_StandardSecurityCM()
        {
            var manifestController = CreateManifestController();

            //Act
            int id = 16;
            var actionResult = manifestController.Get(id) as OkNegotiatedContentResult<CrateDTO>;
            var fieldsList = Deserialize(actionResult);
            ////Assert
            Assert.NotNull(fieldsList);
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("AuthenticateAs")), "AuthenticateAs Not Found");
        }

        [Test]
        public void Check_DocuSignEnvelopeCM()
        {
            //Arrange
            var manifestController = CreateManifestController();

            //Act
            int id = 15;
            var actionResult = manifestController.Get(id) as OkNegotiatedContentResult<CrateDTO>;
            var fieldsList = Deserialize(actionResult);
            ////Assert
            Assert.NotNull(fieldsList);
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("Status")), "Status Not Found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("CreateDate")), "CreateDate Not Found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("SentDate")), "SentDate Not Found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("DeliveredDate")), "DeliveredDate Not Found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("CompletedDate")), "CompletedDate Not Found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("EnvelopeId")), "EnvelopeId Not Found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("ExternalAccountId")), "ExternalAccountId Not Found");
        }

        [Test]
        public void Check_DocuSignEventCM()
        {
            //Arrange
            var manifestController = CreateManifestController();

            //Act
            int id = 14;
            var actionResult = manifestController.Get(id) as OkNegotiatedContentResult<CrateDTO>;
            var fieldsList = Deserialize(actionResult);
            ////Assert
            Assert.NotNull(fieldsList);
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("Status")), "Status Not Found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("Object")), "Object Not Found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("EventId")), "EventId Not Found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("RecepientId")), "RecepientId Not Found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("EnvelopeId")), "EnvelopeId Not Found");
        }

        [Test]
        public void Check_EventReportCM()
        {
            //Arrange
            var manifestController = CreateManifestController();

            //Act
            int id = 7;
            var actionResult = manifestController.Get(id) as OkNegotiatedContentResult<CrateDTO>;
            var fieldsList = Deserialize(actionResult);
            ////Assert
            Assert.NotNull(fieldsList);
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("EventNames")), "EventNames Not Found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("EventPayload")), "EventPayload Not Found");
        }

        [Test]
        public void Check_EventSubscriptionCM()
        {
            var manifestController = CreateManifestController();

            //Act
            int id = 8;
            var actionResult = manifestController.Get(id) as OkNegotiatedContentResult<CrateDTO>;
            var fieldsList = Deserialize(actionResult);
            ////Assert
            Assert.NotNull(fieldsList);
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("Subscriptions")), "Subscriptions Not Found");
        }

        [Test]
        public void Check_StandardConfigurationControlsCM()
        {
            //Arrange
            var manifestController = CreateManifestController();

            //Act
            int id = 6;
            var actionResult = manifestController.Get(id) as OkNegotiatedContentResult<CrateDTO>;
            var fieldsList = Deserialize(actionResult);
            ////Assert
            Assert.NotNull(fieldsList);
           
        }

        [Test]
        public void Check_StandardFileHandleMS()
        {
            //Arrange
            var manifestController = CreateManifestController();

            //Act
            int id = 10;
            var actionResult = manifestController.Get(id) as OkNegotiatedContentResult<CrateDTO>;
            var fieldsList = Deserialize(actionResult);
            ////Assert
            Assert.NotNull(fieldsList);
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("DirectUrl")), "DirectUrl Not Found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("Filename")), "Filename Not Found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("Filetype")), "Filetype Not Found");
        }

        [Test]
        public void Check_StandardPayloadDataCM()
        {
            //Arrange
            var manifestController = CreateManifestController();

            //Act
            int id = 5;
            var actionResult = manifestController.Get(id) as OkNegotiatedContentResult<CrateDTO>;
            var fieldsList = Deserialize(actionResult);
            ////Assert
            Assert.NotNull(fieldsList);
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("PayloadObjects")), "PayloadObjects Not Found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("ObjectType")), "ObjectType Not Found");
        }

        [Test]
        public void Check_StandardRoutingDirectiveCM()
        {
            //Arrange
            var manifestController = CreateManifestController();

            //Act
            int id = 11;
            var actionResult = manifestController.Get(id) as OkNegotiatedContentResult<CrateDTO>;
            var fieldsList = Deserialize(actionResult);
            ////Assert
            Assert.NotNull(fieldsList);
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("Directive")), "Directive Not Found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("TargetProcessNodeName")), "TargetProcessNodeName Not Found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("TargetActivityName")), "TargetActivityName Not Found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("Explanation")), "Explanation Not Found");
        }

        [Test]
        public void Check_StandardTableDataCM()
        {
            //Arrange
            var manifestController = CreateManifestController();

            //Act
            int id = 9;
            var actionResult = manifestController.Get(id) as OkNegotiatedContentResult<CrateDTO>;
            var fieldsList = Deserialize(actionResult);
            ////Assert
            Assert.NotNull(fieldsList);
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("Table")), "Table Not Found");
            Assert.IsTrue(fieldsList.Fields.Any(f => f.Name.Equals("FirstRowHeaders")), "FirstRowHeaders Not Found");
        }

        private static FieldDescriptionsCM Deserialize(OkNegotiatedContentResult<CrateDTO> actionResult)
        {
            var crate = ObjectFactory.GetInstance<ICrateManager>().FromDto(actionResult.Content);

            return crate.Get<FieldDescriptionsCM>();
        }

        private static ManifestsController CreateManifestController()
        {
            return new ManifestsController();
        }
    }
}
