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
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;
using HubWeb.Controllers;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Controllers
{
    [TestFixture]
    [Category("RouteControllerTests")]
    public class RouteControllerTests : BaseTest
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
        public void RouteController_CanAddNewRoute()
        {
            //Arrange 
            var processTemplateDto = FixtureData.CreateTestRouteDTO();

            //Act
            RouteController ptc = CreateRouteController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);
            var response = ptc.Post(processTemplateDto);

            //Assert
            var okResult = response as OkNegotiatedContentResult<RouteOnlyDTO>;
            Assert.NotNull(okResult);
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Assert.AreEqual(0, ptc.ModelState.Count()); //must be no errors
                var ptdo = uow.RouteRepository.
                    GetQuery().SingleOrDefault(pt => pt.Fr8Account.Id == _testUserAccount.Id && pt.Name == processTemplateDto.Name);
                Assert.IsNotNull(ptdo);
                Assert.AreEqual(processTemplateDto.Description, ptdo.Description);
            }
        }

        [Test]
        public void RouteController_Will_Return_BadResult_If_Name_Is_Empty()
        {
            //Arrange 
            var processTemplateDto = FixtureData.CreateTestRouteDTO();
            processTemplateDto.Name = String.Empty;

            //Act
            RouteController ptc = CreateRouteController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address); ;
            var response = ptc.Post(processTemplateDto);

            //Assert
            var badResult = response as BadRequestErrorMessageResult;
            Assert.NotNull(badResult);

        }

        [Test]
        public void RouteController_Will_ReturnEmptyOkResult_If_No_Route_Found()
        {
            //Act
            RouteController processTemplateController = CreateRouteController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);

            //Assert
            var postResult = processTemplateController.Get(FixtureData.GetTestGuidById(55));
            Assert.IsNull(postResult as OkNegotiatedContentResult<RouteDO>);
        }

        [Test]
        public void ProcessController_Will_Return_All_When_Get_Invoked_With_Null()
        {
            //Arrange
            var processTemplateController = CreateRouteController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);
            for (var i = 0; i < 2; i++)
            {
                var processTemplateDto = FixtureData.CreateTestRouteDTO();

                // Commented out by yakov.gnusin:
                // Do we really need to provider DockyardAccountDO inside RouteDTO?
                // We do override DockyardAccountDO in RouteController.Post action.
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
            var actionResult = processTemplateController.Get() as OkNegotiatedContentResult<RouteOnlyDTO[]>;

            //Assert
            Assert.NotNull(actionResult);
            Assert.AreEqual(2, actionResult.Content.Count());
        }

        [Test]
        public void ProcessController_Will_Return_One_When_Get_Invoked_With_Id()
        {
            //Arrange
            var processTemplateController = CreateRouteController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);
            var processTemplateDto = FixtureData.CreateTestRouteDTO();
            processTemplateController.Post(processTemplateDto);

            //Act
            var actionResult = processTemplateController.Get(processTemplateDto.Id) as OkNegotiatedContentResult<RouteOnlyDTO>;

            //Assert
            Assert.NotNull(actionResult);
            Assert.NotNull(actionResult.Content);
            Assert.AreEqual(processTemplateDto.Id, actionResult.Content.Id);

        }

        // MockDB has boken logic when working with collections of objects of derived types
        // We add object to RouteRepository but Delete logic recusively traverse Activity repository.
        [Ignore("MockDB behavior is incorrect")]
        [Test]
        public void RouteController_CanDelete()
        {
            //Arrange 
            var processTemplateDto = FixtureData.CreateTestRouteDTO();

            RouteController processTemplateController = CreateRouteController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);
            var postResult = processTemplateController.Post(processTemplateDto) as OkNegotiatedContentResult<RouteOnlyDTO>;

            Assert.NotNull(postResult);

            //Act
            var deleteResult = processTemplateController.Delete(postResult.Content.Id) as OkNegotiatedContentResult<int>;

            Assert.NotNull(deleteResult);

            //Assert
            //After delete, if we get the same process template, it should be null
            var afterDeleteAttemptResult =
                processTemplateController.Get(postResult.Content.Id) as OkNegotiatedContentResult<RouteOnlyDTO>;
            Assert.IsNull(afterDeleteAttemptResult);
        }


        [Test]
        public void ProcessController_CannotCreateIfProcessNameIsEmpty()
        {
            //Arrange 
            var processTemplateDto = FixtureData.CreateTestRouteDTO();
            processTemplateDto.Name = String.Empty;

            //Act
            RouteController processTemplateController = CreateRouteController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);
            processTemplateController.Post(processTemplateDto);

            //Assert
            Assert.AreEqual(1, processTemplateController.ModelState.Count()); //must be one error
        }

        [Test]
        public void ProcessController_CanEditProcess()
        {
            //Arrange 
            //var processTemplateDto = FixtureData.CreateTestRouteDTO();
            var processTemplateDto = FixtureData.CreateTestRouteDTO();
            var processTemplateController = CreateRouteController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);

            var tPT = FixtureData.TestRouteWithStartingSubroutes_ID0();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.RouteRepository.Add(tPT);
                uow.SaveChanges();
            }

            //Save First
            var postResult = processTemplateController.Post(processTemplateDto) as OkNegotiatedContentResult<RouteOnlyDTO>;
            Assert.NotNull(postResult);

            //Then Get
            var getResult = processTemplateController.Get(postResult.Content.Id) as OkNegotiatedContentResult<RouteOnlyDTO>;
            Assert.NotNull(getResult);

            //Then Edit
            var postEditNameValue = "EditedName";
            getResult.Content.Name = postEditNameValue;
            var editResult = processTemplateController.Post(getResult.Content) as OkNegotiatedContentResult<RouteOnlyDTO>;
            Assert.NotNull(editResult);

            //Then Get
            var postEditGetResult = processTemplateController.Get(editResult.Content.Id) as OkNegotiatedContentResult<RouteOnlyDTO>;
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
            var processTemplateDto = FixtureData.CreateTestRouteDTO();

            var docuSignTemplateList = new List<string>();
            docuSignTemplateList.Add("58521204-58af-4e65-8a77-4f4b51fef626");

            var externalEventList = new List<int?>();
            externalEventList.AddRange(new int?[] { 1, 3 });

            //Act: first add a process template, then modify it. 
            RouteController ptc = CreateRouteController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);
            var response = ptc.Post(processTemplateDto);
            processTemplateDto.Name = "updated";
            processTemplateDto.SubscribedDocuSignTemplates = docuSignTemplateList;
            processTemplateDto.SubscribedExternalEvents = externalEventList;
            response = ptc.Post(processTemplateDto, true);

            //Assert
            var okResult = response as OkNegotiatedContentResult<RouteOnlyDTO>;
            Assert.NotNull(okResult);
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Assert.AreEqual(0, ptc.ModelState.Count()); //must be no errors
                var ptdo = uow.RouteRepository.
                    GetQuery().SingleOrDefault(pt => pt.Fr8Account.Id == _testUserAccount.Id && pt.Name == processTemplateDto.Name);
                Assert.IsNotNull(ptdo);
                Assert.AreEqual(processTemplateDto.Name, ptdo.Name);
                Assert.AreEqual(processTemplateDto.SubscribedDocuSignTemplates.Count(), 1);
                Assert.AreEqual(processTemplateDto.SubscribedDocuSignTemplates[0], docuSignTemplateList[0]);
                Assert.AreEqual(processTemplateDto.SubscribedExternalEvents, externalEventList);
            }
        }
     
        [Test]
        public void ShouldGetFullRoute()
        {
            var curRouteController = new RouteController();
            var curRouteDO = FixtureData.TestRoute3();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.RouteRepository.Add(curRouteDO);
                uow.SaveChanges();
            }

            var curResult = curRouteController.GetFullRoute(curRouteDO.Id) as OkNegotiatedContentResult<RouteDTO>;
            var curRouteDTO = curResult.Content;

            Assert.AreEqual(curRouteDO.Name, curRouteDTO.Name);
            Assert.AreEqual(curRouteDO.Description, curRouteDTO.Description);
            Assert.AreEqual(curRouteDO.Subroutes.Count(), 2);
            Assert.AreEqual(curRouteDO.Subroutes.First().ChildNodes.Count, 1);

        }

        // Current user shoud be resolved using mocked ISecurityServices.
        private static RouteController CreateRouteController(string userId, string email)
        {
            return new RouteController();
        }
    }
}
