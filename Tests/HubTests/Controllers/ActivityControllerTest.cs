using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Results;
using Moq;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using HubTests.Controllers.Api;
using Data.States;
using Hub.Interfaces;
using HubWeb.Controllers;
using UtilitiesTesting.Fixtures;

namespace HubTests.Controllers
{
    [TestFixture]
    [Category("ActionController")]
    public class ActivityControllerTest : ApiControllerTestBase
    {

        private IActivity _activity;

        public ActivityControllerTest()
        {

        }
        public override void SetUp()
        {
            base.SetUp();
            _activity = ObjectFactory.GetInstance<IActivity>();
            // DO-1214
            //CreateEmptyActionList();
            CreateActivityTemplate();
        }

        [Test]
        public void ActivityController_ShouldHaveFr8ApiAuthorize()
        {
            ShouldHaveFr8ApiAuthorize(typeof(ActivitiesController));
        }

        [Test]
        public void ActivityController_ShouldHaveHMACOnCreateMethod()
        {
            var createMethod = typeof(ActivitiesController).GetMethod("Create", new Type[] { typeof(Guid), typeof(string), typeof(string), typeof(int?), typeof(Guid?), typeof(bool), typeof(Guid?) });
            ShouldHaveFr8HMACAuthorizeOnFunction(createMethod);
        }

        [Test]
        public void ActivityController_ShouldHaveHMACOnConfigureMethod()
        {
            ShouldHaveFr8HMACAuthorizeOnFunction(typeof(ActivitiesController), "Configure");
        }

        [Test, Ignore]
        public void ActivityController_ShouldHaveHMACOnDocumentationMethod()
        {
            ShouldHaveFr8HMACAuthorizeOnFunction(typeof(ActivitiesController), "Documentation");
        }

        [Test]
        public async Task ActivityController_Save_WithEmptyActions_NewActionShouldBeCreated()
        {
            var subPlan = FixtureData.TestSubPlanDO1();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = FixtureData.TestPlan1();

                uow.PlanRepository.Add(plan);


                plan.ChildNodes.Add(subPlan);
                uow.SaveChanges();
            }
            //Arrange is done with empty action list

            //Act
            var actualAction = CreateActivityWithId(FixtureData.GetTestGuidById(1));
            actualAction.ParentPlanNodeId = subPlan.Id;

            var controller = new ActivitiesController();
            var result = (OkNegotiatedContentResult<ActivityDTO>)await controller.Save(actualAction);
            var savedAction = result.Content;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Assert
                Assert.IsNotNull(uow.PlanRepository);
                Assert.IsTrue(uow.PlanRepository.GetActivityQueryUncached().Count() == 1);

                var expectedAction = uow.PlanRepository.GetById<ActivityDO>(actualAction.Id);
                Assert.IsNotNull(expectedAction);
                Assert.AreEqual(actualAction.Id, expectedAction.Id);
            }
        }

        [Test]
        public async Task ActivityController_Save_WithActionNotExisting_NewActionShouldBeCreated()
        {
            SubPlanDO subPlan;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = FixtureData.TestPlan1();

                uow.PlanRepository.Add(plan);

                subPlan = FixtureData.TestSubPlanDO1();
                plan.ChildNodes.Add(subPlan);

                //Arrange
                //Add one test action
                var activity = FixtureData.TestActivity1();
                subPlan.ChildNodes.Add(activity);
                uow.SaveChanges();
            }
            //Act
            var actualAction = CreateActivityWithId(FixtureData.GetTestGuidById(2));
            actualAction.ParentPlanNodeId = subPlan.Id;

            var controller = new ActivitiesController();
            var result = (OkNegotiatedContentResult<ActivityDTO>)await controller.Save(actualAction);
            var savedAction = result.Content;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Assert
                Assert.IsNotNull(uow.PlanRepository);
                Assert.AreEqual(2, uow.PlanRepository.GetActivityQueryUncached().Count());

                //Still there is only one action as the update happened.
                var expectedAction = uow.PlanRepository.GetById<ActivityDO>(actualAction.Id);
                Assert.IsNotNull(expectedAction);
                Assert.AreEqual(actualAction.Id, expectedAction.Id);
            }
        }

        [Test]

        public void ActivityController_Save_WithActionExists_ExistingActionShouldBeUpdated()
        {
            //Arrange
            //Add one test action
            var activity = FixtureData.TestActivity1();

            var plan = new PlanDO
            {
                PlanState = PlanState.Active,
                Name = "name",
                ChildNodes = { activity }
            };

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {


                uow.PlanRepository.Add(plan);
                uow.SaveChanges();
            }
            //Act
            var actualAction = CreateActivityWithId(FixtureData.GetTestGuidById(1));

            actualAction.ParentPlanNodeId = plan.Id;

            var controller = new ActivitiesController();
            controller.Save(actualAction);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Assert
                Assert.IsNotNull(uow.PlanRepository);
                Assert.IsTrue(uow.PlanRepository.GetActivityQueryUncached().Count() == 1);

                //Still there is only one action as the update happened.
                var expectedAction = uow.PlanRepository.GetById<ActivityDO>(actualAction.Id);
                Assert.IsNotNull(expectedAction);

            }
        }

        [Test]
        public async Task ActivityController_Delete()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var subPlanMock = new Mock<ISubPlan>();

                subPlanMock.Setup(a => a.DeleteActivity(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);

                ActivityDO activityDO = new FixtureData(uow).TestActivity3();
                var controller = new ActivitiesController(subPlanMock.Object);
                await controller.Delete(activityDO.Id);
                subPlanMock.Verify(a => a.DeleteActivity(null, activityDO.Id, false));
            }
        }

        [Test]

        public void ActivityController_Get()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Mock<IActivity> actionMock = new Mock<IActivity>();
                actionMock.Setup(a => a.GetById(It.IsAny<IUnitOfWork>(), It.IsAny<Guid>()));

                ActivityDO activityDO = new FixtureData(uow).TestActivity3();
                var controller = new ActivitiesController(actionMock.Object);
                controller.Get(activityDO.Id);
                actionMock.Verify(a => a.GetById(It.IsAny<IUnitOfWork>(), activityDO.Id));
            }
        }

        private void CreateActivityTemplate()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ActivityTemplateRepository.Add(FixtureData.TestActivityTemplateDO1());
                uow.SaveChanges();
            }
        }

        private void CreateActivityTemplate(string name, string version)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ActivityTemplateRepository.Add(new ActivityTemplateDO
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Terminal = FixtureData.TerminalTwo(),
                    Version = version
                });
                uow.SaveChanges();
            }
        }

        /// <summary>
        /// Creates a new Action with the given action ID
        /// </summary>
        private ActivityDTO CreateActivityWithId(Guid actionId)
        {
            return new ActivityDTO
            {
                Id = actionId,
                CrateStorage = new CrateStorageDTO(),
                ActivityTemplate = FixtureData.TestActivityTemplateDTOV2()
                //,ActionTemplate = FixtureData.TestActivityTemplateDO2()
            };
        }


        [Test, Ignore]
        public async Task ActivityController_GetConfigurationSettings_ValidActionDesignDTO()
        {

            var controller = new ActivitiesController();
            ActivityDTO actionDesignDTO = CreateActivityWithId(FixtureData.GetTestGuidById(2));
            actionDesignDTO.ActivityTemplate = FixtureData.TestActivityTemplateDTOV2();



            var actionResult = await controller.Configure(actionDesignDTO);

            var okResult = actionResult as OkNegotiatedContentResult<ActivityDO>;

            Assert.IsNotNull(okResult);
            Assert.IsNotNull(okResult.Content);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ApplicationException), ExpectedMessage = "Could not find Action.")]
        public async Task ActivityController_GetConfigurationSettings_IdIsMissing()
        {
            var controller = new ActivitiesController();
            ActivityDTO actionDesignDTO = CreateActivityWithId(FixtureData.GetTestGuidById(2));
            actionDesignDTO.Id = Guid.Empty;

            CreateActivityTemplate(actionDesignDTO.ActivityTemplate.Name, actionDesignDTO.ActivityTemplate.Version);

            var actionResult = await controller.Configure(actionDesignDTO);

            var okResult = actionResult as OkNegotiatedContentResult<ActivityDO>;

            Assert.IsNotNull(okResult);
            Assert.IsNotNull(okResult.Content);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(NullReferenceException))]
        public async Task ActivityController_GetConfigurationSettings_ActionTemplateNameAndVersionIsMissing()
        {
            var controller = new ActivitiesController();
            ActivityDTO actionDesignDTO = CreateActivityWithId(FixtureData.GetTestGuidById(2));
            var actionResult = await controller.Configure(actionDesignDTO);

            var okResult = actionResult as OkNegotiatedContentResult<ActivityDO>;

            Assert.IsNotNull(okResult);
            Assert.IsNotNull(okResult.Content);
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public async Task ActivityController_IncorrectDocumentationSupport()
        {
            var docSupportList = new List<string>
            {
                "Terminal=terminalDocuSign, MainPage, HelpMenu",
                "MainPage, HelpMenu",
                "Terminal=terminalDocuSign, HelpMenu",
                "Terminal=terminalDocuSign, MainPage"
            };
            foreach (var docSupport in docSupportList)
                await ActivityController_IncorrectDocumentationSupport_ThrowsException(docSupport);
        }

        private async Task ActivityController_IncorrectDocumentationSupport_ThrowsException(string docSupport)
        {
            var controller = new ActivitiesController();
            var emptyActivity = new ActivityDTO { Documentation = docSupport };
            var response = await controller.Documentation(emptyActivity);
            var okResult = response as OkNegotiatedContentResult<List<string>>;
        }
    }
}
