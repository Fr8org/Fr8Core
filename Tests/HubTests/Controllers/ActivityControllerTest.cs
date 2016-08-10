using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Results;
using Moq;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using HubTests.Controllers.Api;
using Data.States;
using Fr8.Infrastructure;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Hub.Interfaces;
using HubWeb.Controllers;
using Fr8.Testing.Unit.Fixtures;

namespace HubTests.Controllers
{
    [TestFixture]
    [Category("ActionController")]
    public class ActivityControllerTest : ApiControllerTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
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
        public void ActivityController_ShouldHaveHMACOnConfigureMethod()
        {
            ShouldHaveFr8HMACAuthorizeOnFunction(typeof(ActivitiesController), "Configure");
        }

        [Test]
        public async Task ActivityController_Save_WithEmptyActions_NewActionShouldBeCreated()
        {
            var subPlan = FixtureData.TestSubPlanDO1();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = FixtureData.TestPlan1();

                uow.PlanRepository.Add(plan);
                var activityTemplate = FixtureData.TestActivityTemplateDTOV2();
                var activityTemplateDO = new ActivityTemplateDO
                {
                    Name = activityTemplate.Name,
                    Version = activityTemplate.Version,
                    Terminal = new TerminalDO
                    {
                        Name = activityTemplate.TerminalName,
                        Version = activityTemplate.TerminalVersion,
                        TerminalStatus = TerminalStatus.Active,
                        Label = "dummy",
                        ParticipationState = ParticipationState.Approved,
                        OperationalState = OperationalState.Active,
                        Endpoint = "http://localhost:11111"
                    }
                };
                uow.ActivityTemplateRepository.Add(activityTemplateDO);
                plan.ChildNodes.Add(subPlan);
                uow.SaveChanges();
            }
            //Arrange is done with empty action list

            //Act
            var actualAction = CreateActivityWithId(FixtureData.GetTestGuidById(1));
            actualAction.ParentPlanNodeId = subPlan.Id;

            var controller = ObjectFactory.GetInstance<ActivitiesController>();
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
            SubplanDO subplan;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = FixtureData.TestPlan1();

                uow.PlanRepository.Add(plan);

                subplan = FixtureData.TestSubPlanDO1();
                plan.ChildNodes.Add(subplan);

                var activityTemplate = FixtureData.TestActivityTemplateDTOV2();
                var activityTemplateDO = new ActivityTemplateDO
                {
                    Name = activityTemplate.Name,
                    Version = activityTemplate.Version,
                    Terminal = new TerminalDO
                    {
                        Name = activityTemplate.TerminalName,
                        Version = activityTemplate.TerminalVersion,
                        TerminalStatus = TerminalStatus.Active,
                        Label = "dummy",
                        ParticipationState = ParticipationState.Approved,
                        OperationalState = OperationalState.Active,
                        Endpoint = "http://localhost:11111"
                    }
                    
                };

                uow.ActivityTemplateRepository.Add(activityTemplateDO);

                //Arrange
                //Add one test action
                var activity = FixtureData.TestActivity1();
                subplan.ChildNodes.Add(activity);
                uow.SaveChanges();
            }
            //Act
            var actualAction = CreateActivityWithId(FixtureData.GetTestGuidById(2));
            actualAction.ParentPlanNodeId = subplan.Id;

            var controller = ObjectFactory.GetInstance<ActivitiesController>();
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
                PlanState = PlanState.Executing,
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

            var controller = ObjectFactory.GetInstance<ActivitiesController>();
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
                var activityMock = new Mock<IActivity>();

                activityMock.Setup(a => a.Delete(It.IsAny<Guid>())).Returns(Task.FromResult(0));

                ActivityDO activityDO = new FixtureData(uow).TestActivity3();
                var controller = new ActivitiesController(activityMock.Object, ObjectFactory.GetInstance<IActivityTemplate>(), ObjectFactory.GetInstance<IPlan>(), ObjectFactory.GetInstance<IUnitOfWorkFactory>());
                await controller.Delete(activityDO.Id);
                activityMock.Verify(a => a.Delete(activityDO.Id));
            }
        }

        [Test]

        public void ActivityController_Get()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Mock<IActivity> actionMock = new Mock<IActivity>();
                actionMock.Setup(a => a.GetById(It.IsAny<IUnitOfWork>(), It.IsAny<Guid>()));
                actionMock.Setup(x => x.Exists(It.IsAny<Guid>())).Returns(true);

                ActivityDO activityDO = new FixtureData(uow).TestActivity3();
                var controller = new ActivitiesController(actionMock.Object, ObjectFactory.GetInstance<IActivityTemplate>(), ObjectFactory.GetInstance<IPlan>(), ObjectFactory.GetInstance<IUnitOfWorkFactory>());
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

            var controller = ObjectFactory.GetInstance<ActivitiesController>();
            ActivityDTO actionDesignDTO = CreateActivityWithId(FixtureData.GetTestGuidById(2));
            actionDesignDTO.ActivityTemplate = FixtureData.TestActivityTemplateDTOV2();



            var actionResult = await controller.Configure(actionDesignDTO);

            var okResult = actionResult as OkNegotiatedContentResult<ActivityDO>;

            Assert.IsNotNull(okResult);
            Assert.IsNotNull(okResult.Content);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(MissingObjectException))]
        public async Task ActivityController_GetConfigurationSettings_IdIsMissing()
        { 
            var controller = ObjectFactory.GetInstance<ActivitiesController>();
            ActivityDTO actionDesignDTO = CreateActivityWithId(FixtureData.GetTestGuidById(2));
            actionDesignDTO.Id = Guid.Empty;

            CreateActivityTemplate(actionDesignDTO.ActivityTemplate.Name, actionDesignDTO.ActivityTemplate.Version);

            var actionResult = await controller.Configure(actionDesignDTO);

            var okResult = actionResult as OkNegotiatedContentResult<ActivityDO>;

            Assert.IsNotNull(okResult);
            Assert.IsNotNull(okResult.Content);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(MissingObjectException))]
        public async Task ActivityController_GetConfigurationSettings_ActionTemplateNameAndVersionIsMissing()
        {
            var controller = ObjectFactory.GetInstance<ActivitiesController>();
            ActivityDTO actionDesignDTO = CreateActivityWithId(FixtureData.GetTestGuidById(2));
            var actionResult = await controller.Configure(actionDesignDTO);

            var okResult = actionResult as OkNegotiatedContentResult<ActivityDO>;

            Assert.IsNotNull(okResult);
            Assert.IsNotNull(okResult.Content);
        }
    }
}
