using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Fr8Data.DataTransferObjects;
using HubTests.Controllers.Api;
using HubWeb.Controllers;
using Fr8.Testing.Unit.Fixtures;
using Fr8Data.States;
using HubWeb.ViewModels.RequestParameters;

namespace HubTests.Controllers
{
    [TestFixture]
    [Category("PlansControllerTests")]
    public class PlanControllerTests : ApiControllerTestBase
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
        public void PlansController_ShouldHaveFr8ApiAuthorize()
        {
            ShouldHaveFr8ApiAuthorize(typeof(PlansController));
        }

        //[Test]
        //public void PlansController_ShouldHaveHMACOnCreateMethod()
        //{
        //    var createMethod = typeof(PlansController).GetMethod("Create", new Type[] { typeof(Guid), typeof(string), typeof(string), typeof(int?), typeof(Guid?), typeof(Guid?) });
        //    ShouldHaveFr8HMACAuthorizeOnFunction(createMethod);
        //}

        [Test]
        public void PlansController_ShouldHaveHMACOnPostMethod()
        {

            ShouldHaveFr8HMACAuthorizeOnFunction(typeof(PlansController), "Post");
        }

        [Test]
        public void PlansController_ShouldHaveHMACOnGetMethod()
        {

            ShouldHaveFr8HMACAuthorizeOnFunction(typeof(PlansController), "Get");
        }

        //[Test]
        //public void PlansController_ShouldHaveHMACOnGetByNameMethod()
        //{

        //    ShouldHaveFr8HMACAuthorizeOnFunction(typeof(PlansController), "GetByName");
        //}
        
        [Test]
        public void PlanController_CanAddNewPlan()
        {
            //Arrange 
            var PlanDto = FixtureData.CreateTestPlanDTO();

            //Act
            var ptc = CreatePlanController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);
            var response = ptc.Post(PlanDto).Result;


            //Assert
            var okResult = response as OkNegotiatedContentResult<PlanDTO>;
            Assert.NotNull(okResult);
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Assert.AreEqual(0, ptc.ModelState.Count()); //must be no errors
                var ptdo = uow.PlanRepository.GetPlanQueryUncached().SingleOrDefault(pt => pt.Fr8Account.Id == _testUserAccount.Id && pt.Name == PlanDto.Name);
                Assert.IsNotNull(ptdo);
                Assert.AreEqual(PlanDto.Description, ptdo.Description);
            }
        }

        [Test]
        public void PlanController_Will_Return_BadResult_If_Name_Is_Empty()
        {
            //Arrange 
            var PlanDto = FixtureData.CreateTestPlanDTO();
            PlanDto.Name = String.Empty;

            //Act
            var ptc = CreatePlanController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address); ;
            var response = ptc.Post(PlanDto).Result;

            //Assert
            var badResult = response as BadRequestErrorMessageResult;
            Assert.NotNull(badResult);

        }

        [Test]
        public void PlanController_Will_ReturnEmptyOkResult_If_No_Plan_Found()
        {
            //Act
            PlansController PlanController = CreatePlanController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);

            //Assert
            var postResult = PlanController.Get(new PlansGetParams()
            {
                id = FixtureData.GetTestGuidById(55)
            });
                //FixtureData.GetTestGuidById(55));
            Assert.IsNull(postResult as OkNegotiatedContentResult<PlanDO>);
        }

        [Test]
        public void ProcessController_Will_Return_All_When_Get_Invoked_With_Null()
        {
            //Arrange
            var PlanController = CreatePlanController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);
            for (var i = 0; i < 2; i++)
            {
                var PlanDto = FixtureData.CreateTestPlanDTO();

                // Commented out by yakov.gnusin:
                // Do we really need to provider DockyardAccountDO inside PlanDTO?
                // We do override DockyardAccountDO in PlanController.Post action.
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
                PlanController.Post(PlanDto);
            }
            //Act
            var actionResult = PlanController.Get(new PlansGetParams()) as OkNegotiatedContentResult<IList<PlanEmptyDTO>>;

            //Assert
            Assert.NotNull(actionResult);
            Assert.AreEqual(2, actionResult.Content.Count());
        }

        [Test]
        public void ProcessController_Will_Return_One_When_Get_Invoked_With_Id()
        {
            //Arrange
            var PlanController = CreatePlanController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);
            var PlanDto = FixtureData.CreateTestPlanDTO();
            var resultPlan = (PlanController.Post(PlanDto).Result as OkNegotiatedContentResult<PlanDTO>).Content;

            //Act
            var actionResult = PlanController.Get( new PlansGetParams()
            {
                id = resultPlan.Plan.Id
            }) as OkNegotiatedContentResult<PlanEmptyDTO>;

            //Assert
            Assert.NotNull(actionResult);
            Assert.NotNull(actionResult.Content);
            Assert.AreEqual(resultPlan.Plan.Id, actionResult.Content.Id);

        }

        // MockDB has boken logic when working with collections of objects of derived types
        // We add object to PlanRepository but Delete logic recusively traverse Activity repository.
        [Ignore("MockDB behavior is incorrect")]
        [Test]
        public void PlanController_CanDelete()
        {
            //Arrange 
            var PlanDto = FixtureData.CreateTestPlanDTO();

            var PlanController = CreatePlanController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);
            var postResult = PlanController.Post(PlanDto).Result as OkNegotiatedContentResult<PlanEmptyDTO>;

            Assert.NotNull(postResult);

            //Act
            var deleteResult = PlanController.Delete(postResult.Content.Id) as OkNegotiatedContentResult<int>;

            Assert.NotNull(deleteResult);

            //Assert
            //After delete, if we get the same process template, it should be null
            var afterDeleteAttemptResult =
                PlanController.Get( new PlansGetParams()
                {
                    id = postResult.Content.Id
                }) as OkNegotiatedContentResult<PlanEmptyDTO>;
            Assert.IsNull(afterDeleteAttemptResult);
        }


        [Test]
        public void ProcessController_CannotCreateIfProcessNameIsEmpty()
        {
            //Arrange 
            var PlanDto = FixtureData.CreateTestPlanDTO();
            PlanDto.Name = String.Empty;

            //Act
            var PlanController = CreatePlanController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);
            PlanController.Post(PlanDto);

            //Assert
            Assert.AreEqual(1, PlanController.ModelState.Count()); //must be one error
        }

        [Test]
        public void ProcessController_CanEditProcess()
        {
            //Arrange 
            //var processTemplateDto = FixtureData.CreateTestPlanDTO();
            var PlanDto = FixtureData.CreateTestPlanDTO();
            var PlanController = CreatePlanController(_testUserAccount.Id, _testUserAccount.EmailAddress.Address);

            var tPT = FixtureData.TestPlanWithStartingSubPlans_ID0();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.PlanRepository.Add(tPT);
                uow.SaveChanges();
            }

            //Save First
            var postResult = PlanController.Post(PlanDto).Result as OkNegotiatedContentResult<PlanDTO>;
            Assert.NotNull(postResult);

            //Then Get
            var getResult = PlanController.Get(new PlansGetParams()
            {
                id = postResult.Content.Plan.Id
            }) as OkNegotiatedContentResult<PlanEmptyDTO>;
            Assert.NotNull(getResult);

            //Then Edit
            var postEditNameValue = "EditedName";
            getResult.Content.Name = postEditNameValue;
            var editResult = PlanController.Post(getResult.Content).Result as OkNegotiatedContentResult<PlanDTO>;
            Assert.NotNull(editResult);

            //Then Get
            var postEditGetResult = PlanController.Get( new PlansGetParams()
            {
                id = editResult.Content.Plan.Id
            }) as OkNegotiatedContentResult<PlanEmptyDTO>;
            Assert.NotNull(postEditGetResult);

            //Assert 
            Assert.AreEqual(postEditGetResult.Content.Name, postEditNameValue);
            Assert.AreEqual(postEditGetResult.Content.Id, editResult.Content.Plan.Id);
            Assert.AreEqual(postEditGetResult.Content.Id, postResult.Content.Plan.Id);
        }

        
        [Test]
        public void ShouldGetFullPlan()
        {
            var curPlanController = new PlansController();
            var curPlanDO = FixtureData.TestPlan3();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                curPlanDO.Fr8Account = FixtureData.TestDeveloperAccount();
                uow.ActivityTemplateRepository.Add(new ActivityTemplateDO
                {
                    TerminalId = 1,

                    Id = FixtureData.GetTestGuidById(1),
                    Name = "New template",
                });


                uow.TerminalRepository.Add(new TerminalDO()
                {
                    Id = 1,
                    TerminalStatus = TerminalStatus.Active,
                    Name = "terminal",
                    Label = "term",
                    Version = "1"

                });
                uow.UserRepository.Add(curPlanDO.Fr8Account);
                uow.PlanRepository.Add(curPlanDO);
                uow.SaveChanges();
            }

            //var curResult = curPlanController.GetFullPlan(curPlanDO.Id) as OkNegotiatedContentResult<PlanDTO>;
            var curResult = curPlanController.Get( new PlansGetParams()
            {
                id = curPlanDO.Id,
                include_children = true
            })
                as OkNegotiatedContentResult<PlanDTO>;

            var curPlanDTO = curResult.Content.Plan;

            Assert.AreEqual(curPlanDO.Name, curPlanDTO.Name);
            Assert.AreEqual(curPlanDO.Description, curPlanDTO.Description);
            Assert.AreEqual(curPlanDO.SubPlans.Count(), 2);
            Assert.AreEqual(curPlanDO.SubPlans.First().ChildNodes.Count, 1);

        }

        // Current user shoud be resolved using mocked ISecurityServices.
        private static PlansController CreatePlanController(string userId, string email)
        {
            return new PlansController();
        }
    }
}
