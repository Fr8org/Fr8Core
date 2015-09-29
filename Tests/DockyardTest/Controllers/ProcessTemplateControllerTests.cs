using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Claims;
using System.Security.Principal;
using System.Web.Http.Results;
using NUnit.Framework;
using StructureMap;
using StructureMap.AutoMocking;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Web.Controllers;

namespace DockyardTest.Controllers
{
    [TestFixture]
    [Category("Controllers.Api.ProcessTemplateService")]
    public class ProcessTemplateControllerTests : BaseTest
    {
        private DockyardAccountDO _testUserAccount;


        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _testUserAccount = FixtureData.TestUser1();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.UserRepository.Add(_testUserAccount);
                uow.SaveChanges();
            }
        }

        [TearDown]
        public void TearDown()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curUser = uow.UserRepository.GetQuery()
                    .SingleOrDefault(x => x.Id == _testUserAccount.Id);

                uow.UserRepository.Remove(curUser);
                uow.SaveChanges();
            }
        }

        [Test]
        public void ProcessTemplateController_CanAddNewProcessTemplate()
        {
            //Arrange 
            var processTemplateDto = FixtureData.CreateTestProcessTemplateDTO();

            //Act
            ProcessTemplateController ptc = CreateProcessTemplateController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);
            var response = ptc.Post(processTemplateDto);

            //Assert
            var okResult = response as OkNegotiatedContentResult<ProcessTemplateOnlyDTO>;
            Assert.NotNull(okResult);
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Assert.AreEqual(0, ptc.ModelState.Count()); //must be no errors
                var ptdo = uow.ProcessTemplateRepository.
                    GetQuery().SingleOrDefault(pt => pt.DockyardAccount.Id == _testUserAccount.Id && pt.Name == processTemplateDto.Name);
                Assert.IsNotNull(ptdo);
                Assert.AreEqual(processTemplateDto.Description, ptdo.Description);
            }
        }

        [Test]
        public void ProcessTemplateController_Will_Return_BadResult_If_Name_Is_Empty()
        {
            //Arrange 
            var processTemplateDto = FixtureData.CreateTestProcessTemplateDTO();
            processTemplateDto.Name = String.Empty;

            //Act
            ProcessTemplateController ptc = CreateProcessTemplateController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address); ;
            var response = ptc.Post(processTemplateDto);

            //Assert
            var badResult = response as BadRequestErrorMessageResult;
            Assert.NotNull(badResult);

        }

        [Test]
        public void ProcessTemplateController_Will_ReturnEmptyOkResult_If_No_ProcessTemplate_Found()
        {
            //Act
            ProcessTemplateController processTemplateController = CreateProcessTemplateController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);

            //Assert
            var postResult = processTemplateController.Get(55);
            Assert.IsNull(postResult as OkNegotiatedContentResult<ProcessTemplateDO>);
        }

        [Test]
        public void ProcessController_Will_Return_All_When_Get_Invoked_With_Null()
        {
            //Arrange
            var processTemplateController = CreateProcessTemplateController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);
            for (var i = 0; i < 2; i++)
            {
                var processTemplateDto = FixtureData.CreateTestProcessTemplateDTO();
                // Commented out by yakov.gnusin:
                // Do we really need to provider DockyardAccountDO inside ProcessTemplateDTO?
                // We do override DockyardAccountDO in ProcessTemplateController.Post action.
                // switch (i)
                // {
                //     case 0:
                //         processTemplateDto.DockyardAccount = FixtureData.TestDockyardAccount1();
                //         break;
                //     case 1:
                //         processTemplateDto.DockyardAccount = FixtureData.TestDockyardAccount2();
                //         break;
                //     default:
                //         break;
                // }
                processTemplateController.Post(processTemplateDto);
            }

            //Act
            var actionResult = processTemplateController.Get() as OkNegotiatedContentResult<IEnumerable<ProcessTemplateOnlyDTO>>;

            //Assert
            Assert.NotNull(actionResult);
            Assert.AreEqual(2, actionResult.Content.Count());
        }

        [Test]
        public void ProcessController_Will_Return_One_When_Get_Invoked_With_Id()
        {
            //Arrange
            var processTemplateController = CreateProcessTemplateController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);
            var processTemplateDto = FixtureData.CreateTestProcessTemplateDTO();
            processTemplateController.Post(processTemplateDto);

            //Act
            var actionResult = processTemplateController.Get(processTemplateDto.Id) as OkNegotiatedContentResult<ProcessTemplateOnlyDTO>;

            //Assert
            Assert.NotNull(actionResult);
            Assert.NotNull(actionResult.Content);
            Assert.AreEqual(processTemplateDto.Id, actionResult.Content.Id);

        }

        [Test]
        public void ProcessTemplateController_CanDelete()
        {
            //Arrange 
            var processTemplateDto = FixtureData.CreateTestProcessTemplateDTO();

            ProcessTemplateController processTemplateController = CreateProcessTemplateController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);
            var postResult = processTemplateController.Post(processTemplateDto) as OkNegotiatedContentResult<ProcessTemplateOnlyDTO>;

            Assert.NotNull(postResult);

            //Act
            var deleteResult = processTemplateController.Delete(postResult.Content.Id) as OkNegotiatedContentResult<int>;

            Assert.NotNull(deleteResult);

            //Assert
            //After delete, if we get the same process template, it should be null
            var afterDeleteAttemptResult =
                processTemplateController.Get(postResult.Content.Id) as OkNegotiatedContentResult<ProcessTemplateOnlyDTO>;
            Assert.IsNull(afterDeleteAttemptResult);
        }


        [Test]
        public void ProcessController_CannotCreateIfProcessNameIsEmpty()
        {
            //Arrange 
            var processTemplateDto = FixtureData.CreateTestProcessTemplateDTO();
            processTemplateDto.Name = String.Empty;

            //Act
            ProcessTemplateController processTemplateController = CreateProcessTemplateController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);
            processTemplateController.Post(processTemplateDto);

            //Assert
            Assert.AreEqual(1, processTemplateController.ModelState.Count()); //must be one error
        }

        [Test]
        public void ProcessController_CanEditProcess()
        {
            //Arrange 
            var processTemplateDto = FixtureData.CreateTestProcessTemplateDTO();
            var processTemplateController = CreateProcessTemplateController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);
            
            //Save First
            var postResult = processTemplateController.Post(processTemplateDto) as OkNegotiatedContentResult<ProcessTemplateOnlyDTO>;
            Assert.NotNull(postResult);

            //Then Get
            var getResult = processTemplateController.Get(postResult.Content.Id) as OkNegotiatedContentResult<ProcessTemplateOnlyDTO>;
            Assert.NotNull(getResult);

            //Then Edit
            var postEditNameValue = "EditedName";
            getResult.Content.Name = postEditNameValue;
            var editResult = processTemplateController.Post(getResult.Content) as OkNegotiatedContentResult<ProcessTemplateOnlyDTO>;
            Assert.NotNull(editResult);

            //Then Get
            var postEditGetResult = processTemplateController.Get(editResult.Content.Id) as OkNegotiatedContentResult<ProcessTemplateOnlyDTO>;
            Assert.NotNull(postEditGetResult);

            //Assert 
            Assert.AreEqual(postEditGetResult.Content.Name,postEditNameValue);
            Assert.AreEqual(postEditGetResult.Content.Id,editResult.Content.Id);
            Assert.AreEqual(postEditGetResult.Content.Id, postResult.Content.Id);
            Assert.AreEqual(postEditGetResult.Content.Id, getResult.Content.Id);            
        }

        [Test,Ignore]
        public void ProcessController_CanUpdateDocuSignTemplate()
        {
            //Arrange
            var processTemplateDto = FixtureData.CreateTestProcessTemplateDTO();

            var docuSignTemplateList = new List<string>();
            docuSignTemplateList.Add("58521204-58af-4e65-8a77-4f4b51fef626");

            var externalEventList = new List<int?>();
            externalEventList.AddRange(new int?[] { 1, 3 });

            //Act: first add a process template, then modify it. 
            ProcessTemplateController ptc = CreateProcessTemplateController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);
            var response = ptc.Post(processTemplateDto);
            processTemplateDto.Name = "updated";
            processTemplateDto.SubscribedDocuSignTemplates = docuSignTemplateList;
            processTemplateDto.SubscribedExternalEvents = externalEventList;
            response = ptc.Post(processTemplateDto, true);

            //Assert
            var okResult = response as OkNegotiatedContentResult<ProcessTemplateOnlyDTO>;
            Assert.NotNull(okResult);
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Assert.AreEqual(0, ptc.ModelState.Count()); //must be no errors
                var ptdo = uow.ProcessTemplateRepository.
                    GetQuery().SingleOrDefault(pt => pt.DockyardAccount.Id == _testUserAccount.Id && pt.Name == processTemplateDto.Name);
                Assert.IsNotNull(ptdo);
                Assert.AreEqual(processTemplateDto.Name, ptdo.Name);
                Assert.AreEqual(processTemplateDto.SubscribedDocuSignTemplates.Count(), 1);
                Assert.AreEqual(processTemplateDto.SubscribedDocuSignTemplates[0], docuSignTemplateList[0]);
                Assert.AreEqual(processTemplateDto.SubscribedExternalEvents, externalEventList);
            }
        }

        [Test, Ignore("Ignored as part of External Event Type removal. This is handled in V2 Event Handling mechanism.")]
        public void ProcessController_CanGetExternalEventList()
        {
            ProcessTemplateController ptc = CreateProcessTemplateController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);
            var triggerSettings = ptc.GetTriggerSettings() as OkNegotiatedContentResult<List<ExternalEventDTO>>;
            Assert.AreEqual(4, triggerSettings.Content.Count);
        }

        [Test]
        public void ShouldGetFullProcessTemplate()
        {
            var curProcessTemplateController = new ProcessTemplateController();
            var curProcessTemplateDO = FixtureData.TestProcessTemplate3();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ProcessTemplateRepository.Add(curProcessTemplateDO);
                uow.SaveChanges();
            }

            var curResult = curProcessTemplateController.GetFullProcessTemplate(curProcessTemplateDO.Id) as OkNegotiatedContentResult<ProcessTemplateDTO>;
            var curProcessTemplateDTO = curResult.Content;

            Assert.AreEqual(curProcessTemplateDO.Name, curProcessTemplateDTO.Name);
            Assert.AreEqual(curProcessTemplateDO.Description, curProcessTemplateDTO.Description);
            Assert.AreEqual(curProcessTemplateDO.ProcessNodeTemplates.Count, 2);
            Assert.AreEqual(curProcessTemplateDO.ProcessNodeTemplates[0].ActionLists.Count, 1);
            Assert.AreEqual(curProcessTemplateDO.ProcessNodeTemplates[0].ActionLists[0].Activities.Count, 1);

        }


        private static ProcessTemplateController CreateProcessTemplateController(string userId, string email)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            claims.Add(new Claim(ClaimTypes.Name, email));
            claims.Add(new Claim(ClaimTypes.Email, email));

            var identity = new ClaimsIdentity(claims);

            var ptc = new ProcessTemplateController
            {
                User = new GenericPrincipal(identity, new[] { "Users" })
            };

            return ptc;
        }
    }
}
