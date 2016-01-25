using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
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
            var routeDto = FixtureData.CreateTestRouteDTO();

            //Act
            var ptc = CreateRouteController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);
            var response = ptc.Post(routeDto);

            
            //Assert
            var okResult = response as OkNegotiatedContentResult<RouteFullDTO>;
            Assert.NotNull(okResult);
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Assert.AreEqual(0, ptc.ModelState.Count()); //must be no errors
                var ptdo = uow.RouteRepository.
                    GetQuery().SingleOrDefault(pt => pt.Fr8Account.Id == _testUserAccount.Id && pt.Name == routeDto.Name);
                Assert.IsNotNull(ptdo);
                Assert.AreEqual(routeDto.Description, ptdo.Description);
            }
        }

        [Test]
        public void RouteController_Will_Return_BadResult_If_Name_Is_Empty()
        {
            //Arrange 
            var routeDto = FixtureData.CreateTestRouteDTO();
            routeDto.Name = String.Empty;

            //Act
            var ptc = CreateRouteController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address); ;
            var response = ptc.Post(routeDto);

            //Assert
            var badResult = response as BadRequestErrorMessageResult;
            Assert.NotNull(badResult);

        }

        [Test]
        public void RouteController_Will_ReturnEmptyOkResult_If_No_Route_Found()
        {
            //Act
            RoutesController routeController = CreateRouteController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);

            //Assert
            var postResult = routeController.Get(FixtureData.GetTestGuidById(55));
            Assert.IsNull(postResult as OkNegotiatedContentResult<PlanDO>);
        }

        [Test]
        public void ProcessController_Will_Return_All_When_Get_Invoked_With_Null()
        {
            //Arrange
            var routeController = CreateRouteController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);
            for (var i = 0; i < 2; i++)
            {
                var routeDto = FixtureData.CreateTestRouteDTO();

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
                routeController.Post(routeDto);
            }

            //Act
            var actionResult = routeController.Get() as OkNegotiatedContentResult<RouteEmptyDTO[]>;

            //Assert
            Assert.NotNull(actionResult);
            Assert.AreEqual(2, actionResult.Content.Count());
        }

        [Test]
        public void ProcessController_Will_Return_One_When_Get_Invoked_With_Id()
        {
            //Arrange
            var routeController = CreateRouteController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);
            var routeDto = FixtureData.CreateTestRouteDTO();
            var resultRoute = (routeController.Post(routeDto) as OkNegotiatedContentResult<RouteFullDTO>).Content;

            //Act
            var actionResult = routeController.Get(resultRoute.Id) as OkNegotiatedContentResult<RouteEmptyDTO>;

            //Assert
            Assert.NotNull(actionResult);
            Assert.NotNull(actionResult.Content);
            Assert.AreEqual(resultRoute.Id, actionResult.Content.Id);

        }

        // MockDB has boken logic when working with collections of objects of derived types
        // We add object to RouteRepository but Delete logic recusively traverse Activity repository.
        [Ignore("MockDB behavior is incorrect")]
        [Test]
        public void RouteController_CanDelete()
        {
            //Arrange 
            var routeDto = FixtureData.CreateTestRouteDTO();

            var routeController = CreateRouteController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);
            var postResult = routeController.Post(routeDto) as OkNegotiatedContentResult<RouteEmptyDTO>;

            Assert.NotNull(postResult);

            //Act
            var deleteResult = routeController.Delete(postResult.Content.Id) as OkNegotiatedContentResult<int>;

            Assert.NotNull(deleteResult);

            //Assert
            //After delete, if we get the same process template, it should be null
            var afterDeleteAttemptResult =
                routeController.Get(postResult.Content.Id) as OkNegotiatedContentResult<RouteEmptyDTO>;
            Assert.IsNull(afterDeleteAttemptResult);
        }


        [Test]
        public void ProcessController_CannotCreateIfProcessNameIsEmpty()
        {
            //Arrange 
            var routeDto = FixtureData.CreateTestRouteDTO();
            routeDto.Name = String.Empty;

            //Act
            var routeController = CreateRouteController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);
            routeController.Post(routeDto);

            //Assert
            Assert.AreEqual(1, routeController.ModelState.Count()); //must be one error
        }

        [Test]
        public void ProcessController_CanEditProcess()
        {
            //Arrange 
            //var processTemplateDto = FixtureData.CreateTestRouteDTO();
            var routeDto = FixtureData.CreateTestRouteDTO();
            var routeController = CreateRouteController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);

            var tPT = FixtureData.TestRouteWithStartingSubroutes_ID0();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.RouteRepository.Add(tPT);
                uow.SaveChanges();
            }

            //Save First
            var postResult = routeController.Post(routeDto) as OkNegotiatedContentResult<RouteFullDTO>;
            Assert.NotNull(postResult);

            //Then Get
            var getResult = routeController.Get(postResult.Content.Id) as OkNegotiatedContentResult<RouteEmptyDTO>;
            Assert.NotNull(getResult);

            //Then Edit
            var postEditNameValue = "EditedName";
            getResult.Content.Name = postEditNameValue;
            var editResult = routeController.Post(getResult.Content) as OkNegotiatedContentResult<RouteFullDTO>;
            Assert.NotNull(editResult);

            //Then Get
            var postEditGetResult = routeController.Get(editResult.Content.Id) as OkNegotiatedContentResult<RouteEmptyDTO>;
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
            var routeDto = FixtureData.CreateTestRouteDTO();

            var docuSignTemplateList = new List<string>();
            docuSignTemplateList.Add("58521204-58af-4e65-8a77-4f4b51fef626");

            var externalEventList = new List<int?>();
            externalEventList.AddRange(new int?[] { 1, 3 });

            //Act: first add a process template, then modify it. 
            RoutesController ptc = CreateRouteController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);
            var response = ptc.Post(routeDto);
            routeDto.Name = "updated";
            response = ptc.Post(routeDto, true);

            //Assert
            var okResult = response as OkNegotiatedContentResult<RouteEmptyDTO>;
            Assert.NotNull(okResult);
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Assert.AreEqual(0, ptc.ModelState.Count()); //must be no errors
                var ptdo = uow.RouteRepository.
                    GetQuery().SingleOrDefault(pt => pt.Fr8Account.Id == _testUserAccount.Id && pt.Name == routeDto.Name);
                Assert.IsNotNull(ptdo);
                Assert.AreEqual(routeDto.Name, ptdo.Name);
            }
        }
     
        [Test]
        public void ShouldGetFullRoute()
        {
            var curRouteController = new RoutesController();
            var curPlanDO = FixtureData.TestRoute3();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                curPlanDO.Fr8Account = FixtureData.TestDeveloperAccount();
                uow.RouteRepository.Add(curPlanDO);
                uow.SaveChanges();
            }

            var curResult = curRouteController.GetFullRoute(curPlanDO.Id) as OkNegotiatedContentResult<RouteFullDTO>;
            var curRouteDTO = curResult.Content;

            Assert.AreEqual(curPlanDO.Name, curRouteDTO.Name);
            Assert.AreEqual(curPlanDO.Description, curRouteDTO.Description);
            Assert.AreEqual(curPlanDO.Subroutes.Count(), 2);
            Assert.AreEqual(curPlanDO.Subroutes.First().ChildNodes.Count, 1);

        }

        // Current user shoud be resolved using mocked ISecurityServices.
        private static RoutesController CreateRouteController(string userId, string email)
        {
            return new RoutesController();
        }
    }
}
