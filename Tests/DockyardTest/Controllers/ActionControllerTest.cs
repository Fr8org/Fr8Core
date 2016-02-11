using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using AutoMapper;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using DockyardTest.Controllers.Api;
using Data.States;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Services;
using HubWeb;
using HubWeb.Controllers;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Controllers
{
    [TestFixture]
    [Category("ActionController")]
    public class ActionControllerTest : ApiControllerTestBase
    {

        private IActivity _activity;

        public ActionControllerTest()
        {

        }
        public override void SetUp()
        {
            base.SetUp();
            _activity = ObjectFactory.GetInstance<IActivity>();
            // DO-1214
            //CreateEmptyActionList();
            CreateActionTemplate();
        }

        [Test]
        public void ActionController_ShouldHaveFr8ApiAuthorize()
        {
            ShouldHaveFr8ApiAuthorize(typeof(ActionsController));
        }

        [Test]
        public void ActionController_ShouldHaveHMACOnCreateMethod()
        {
            var createMethod = typeof (ActionsController).GetMethod("Create", new Type[] { typeof(int), typeof(string), typeof(string), typeof(int ?), typeof(Guid ?), typeof(bool), typeof(Guid ?)});
            ShouldHaveFr8HMACAuthorizeOnFunction(createMethod);
        }

        [Test]
        public void ActionController_ShouldHaveHMACOnConfigureMethod()
        {
            ShouldHaveFr8HMACAuthorizeOnFunction(typeof(ActionsController), "Configure");
        }

        [Test,Ignore]
        public void ActionController_ShouldHaveHMACOnDocumentationMethod()
        {
            ShouldHaveFr8HMACAuthorizeOnFunction(typeof(ActionsController), "Documentation");
        }

        [Test]
        public void ActionController_Save_WithEmptyActions_NewActionShouldBeCreated()
        {
            var subroute = FixtureData.TestSubrouteDO1();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = FixtureData.TestRoute1();

                uow.PlanRepository.Add(plan);


                plan.ChildNodes.Add(subroute);
                uow.SaveChanges();
            }
            //Arrange is done with empty action list

            //Act
            var actualAction = CreateActionWithId(FixtureData.GetTestGuidById(1));
            actualAction.ParentRouteNodeId = subroute.Id;
                
            var controller = new ActionsController();
            var result = (OkNegotiatedContentResult<ActivityDTO>) controller.Save(actualAction);
            var savedAction = result.Content;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Assert
                Assert.IsNotNull(uow.PlanRepository);
                Assert.IsTrue(uow.PlanRepository.GetActivityQueryUncached().Count() == 1);

                var expectedAction = uow.PlanRepository.GetById<ActivityDO>(actualAction.Id);
                Assert.IsNotNull(expectedAction);
                Assert.AreEqual(actualAction.Name, expectedAction.Name);
            }
        }

        [Test]
        public void ActionController_Save_WithActionNotExisting_NewActionShouldBeCreated()
        {
            SubrouteDO subroute;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = FixtureData.TestRoute1();

                uow.PlanRepository.Add(plan);

                subroute = FixtureData.TestSubrouteDO1();
                plan.ChildNodes.Add(subroute);

                //Arrange
                //Add one test action
                var activity = FixtureData.TestActivity1();
                subroute.ChildNodes.Add(activity);
                uow.SaveChanges();
            }
                //Act
                var actualAction = CreateActionWithId(FixtureData.GetTestGuidById(2));
                actualAction.ParentRouteNodeId = subroute.Id;

                var controller = new ActionsController();
                var result = (OkNegotiatedContentResult<ActivityDTO>) controller.Save(actualAction);
                var savedAction = result.Content;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Assert
                Assert.IsNotNull(uow.PlanRepository);
                Assert.AreEqual(2, uow.PlanRepository.GetActivityQueryUncached().Count());

                //Still there is only one action as the update happened.
                var expectedAction = uow.PlanRepository.GetById<ActivityDO>(actualAction.Id);
                Assert.IsNotNull(expectedAction);
                Assert.AreEqual(actualAction.Name, expectedAction.Name);
            }
        }

        [Test]

        public void ActionController_Save_WithActionExists_ExistingActionShouldBeUpdated()
        {
                //Arrange
                //Add one test action
                var activity = FixtureData.TestActivity1();

            var plan = new PlanDO
            {
                RouteState = RouteState.Active,
                Name = "name",
                ChildNodes = {activity}
            };

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {


                uow.PlanRepository.Add(plan);
                uow.SaveChanges();
            }
                //Act
                var actualAction = CreateActionWithId(FixtureData.GetTestGuidById(1));

            actualAction.ParentRouteNodeId = plan.Id;

                var controller = new ActionsController();
                controller.Save(actualAction);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Assert
                Assert.IsNotNull(uow.PlanRepository);
                Assert.IsTrue(uow.PlanRepository.GetActivityQueryUncached().Count() == 1);

                //Still there is only one action as the update happened.
                var expectedAction = uow.PlanRepository.GetById<ActivityDO>(actualAction.Id);
                Assert.IsNotNull(expectedAction);
                Assert.AreEqual(expectedAction.Name, actualAction.Name);
            }
        }


        [Test]

        [Ignore("The real server is not in execution in AppVeyor. Remove these tests once Jasmine Front End integration tests are added.")]
        public async void ActionController_Configure_WithoutConnectionString_ShouldReturnOneEmptyConnectionString()
        {
//            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
//            {
//                //Arrange
//                //remvoe existing action templates
//                uow.ActivityTemplateRepository.Remove(uow.ActivityTemplateRepository.GetByKey(1));
//                uow.SaveChanges();
//
//                //create action
//                var curAction = CreateActionWithV2ActionTemplate(uow);
//                curAction.CrateStorage = JsonConvert.SerializeObject(FixtureData.TestConfigurationStore());
//                uow.SaveChanges();
//
//                var curActionDesignDO = Mapper.Map<ActionDTO>(curAction);
//                //Act
//                var result = await
//                    new ActionController(_action).Configure(curActionDesignDO) as
//                        OkNegotiatedContentResult<string>;
//
//                CrateStorageDTO resultantCrateStorageDto =
//                    JsonConvert.DeserializeObject<CrateStorageDTO>(result.Content);
//
//                //Assert
//                Assert.IsNotNull(result, "Configure POST reqeust is failed");
//                Assert.IsNotNull(resultantCrateStorageDto, "Configure returns no Configuration Store");
//                Assert.IsTrue(resultantCrateStorageDto.CrateDTO.Count == 1, "Configure is not assuming this is the first request from the client");
//                //different V2 format
//                //Assert.AreEqual("connection_string", resultantCrateStorageDto.Fields[0].Name, "Configure does not return one connection string with empty value");
//                //Assert.IsEmpty(resultantCrateStorageDto.Fields[0].Value, "Configure returned some connectoin string when the first request made");
//                
//                ////There should be no data fields as this is the first request from the client
//                //Assert.IsTrue(resultantCrateStorageDto.DataFields.Count == 0, "Configure did not assume this is the first call from the client");
//            }
        }

        [Test]

        [Ignore("The real server is not in execution in AppVeyor. Remove these tests once Jasmine Front End integration tests are added.")]
        public async void ActionController_Configure_WithConnectionString_ShouldReturnDataFields()
        {
//            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
//            {
//                //Arrange
//                //remvoe existing action templates
//                uow.ActivityTemplateRepository.Remove(uow.ActivityTemplateRepository.GetByKey(1));
//                uow.SaveChanges();
//
//                //create action
//                var curAction = CreateActionWithV2ActionTemplate(uow);
//                var configurationStore = FixtureData.TestConfigurationStore();
//                //different V2 format
//                //configurationStore.Fields[0].Value = "Data Source=s79ifqsqga.database.windows.net;database=demodb_health;User ID=alexeddodb;Password=Thales89;";
//                curAction.CrateStorage = JsonConvert.SerializeObject(configurationStore);
//                uow.SaveChanges();
//                var curActionDesignDO = Mapper.Map<ActionDTO>(curAction);
//                //Act
//                var result = await
//                    new ActionController(_action).Configure(curActionDesignDO) as
//                        OkNegotiatedContentResult<string>;
//
//                CrateStorageDTO resultantCrateStorageDto =
//                    JsonConvert.DeserializeObject<CrateStorageDTO>(result.Content);
//
//                //Assert
//                Assert.IsNotNull(result, "Configure POST reqeust is failed");
//                Assert.IsNotNull(resultantCrateStorageDto, "Configure returns no Configuration Store");
//                Assert.IsTrue(resultantCrateStorageDto.CrateDTO.Count == 3, "Configure returned invalid data fields");
//            }
        }

        [Test]
        [Ignore("The real server is not in execution in AppVeyor. Remove these tests once Jasmine Front End integration tests are added.")]
        public async void ActionController_Configure_WithConnectionStringAndDataFields_ShouldReturnUpdatedDataFields()
        {
//            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
//            {
//                //Arrange
//                //remvoe existing action templates
//                uow.ActivityTemplateRepository.Remove(uow.ActivityTemplateRepository.GetByKey(1));
//                uow.SaveChanges();
//
//                //create action
//                var curAction = CreateActionWithV2ActionTemplate(uow);
//                var configurationStore = FixtureData.TestConfigurationStore();
//                //V2 changes
//                //configurationStore.Fields[0].Value = "Data Source=s79ifqsqga.database.windows.net;database=demodb_health;User ID=alexeddodb;Password=Thales89;";
//                //configurationStore.DataFields.Add("something");
//                //configurationStore.DataFields.Add("Wrong");
//                //configurationStore.DataFields.Add("data fields");
//                //configurationStore.DataFields.Add("data fields");
//                curAction.CrateStorage = JsonConvert.SerializeObject(configurationStore);
//                uow.SaveChanges();
//                var curActionDesignDO = Mapper.Map<ActionDTO>(curAction);
//                //Act
//                var result = await
//                    new ActionController(_action).Configure(curActionDesignDO) as
//                        OkNegotiatedContentResult<string>;
//
//                CrateStorageDTO resultantCrateStorageDto =
//                    JsonConvert.DeserializeObject<CrateStorageDTO>(result.Content);
//
//                //Assert
//                Assert.IsNotNull(result, "Configure POST reqeust is failed");
//                Assert.IsNotNull(resultantCrateStorageDto, "Configure returns no Configuration Store");
//                //V2 changes
//                //Assert.IsTrue(resultantCrateStorageDto.DataFields.Count != 4, "Since we already had 4 invalid data fields, the number of data fields should not be 4 now.");
//                //Assert.IsTrue(resultantCrateStorageDto.DataFields.Count == 3, "The new data field should be 3 data fields as with the update one.");
//            }
        }

        [Test]

        public async void ActionController_Delete()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var subRouteMock = new Mock<ISubroute>();

                subRouteMock.Setup(a => a.DeleteActivity(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);

                ActivityDO activityDO = new FixtureData(uow).TestActivity3();
                var controller = new ActionsController(subRouteMock.Object);
                await controller.Delete(activityDO.Id);
                subRouteMock.Verify(a => a.DeleteActivity(null, activityDO.Id, false));
            }
        }

        [Test]

        public void ActionController_Get()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Mock<IActivity> actionMock = new Mock<IActivity>();
                actionMock.Setup(a => a.GetById(It.IsAny<IUnitOfWork>(), It.IsAny<Guid>()));

                ActivityDO activityDO = new FixtureData(uow).TestActivity3();
                var controller = new ActionsController(actionMock.Object);
                controller.Get(activityDO.Id);
                actionMock.Verify(a => a.GetById(It.IsAny<IUnitOfWork>(), activityDO.Id));
            }
        }

        /// <summary>
        /// Creates one empty action list
        /// </summary>
        // DO-1214
//        private void CreateEmptyActionList()
//        {
//            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
//            {
//                // 
//                var curPlan = FixtureData.TestRoute1();
//                uow.RouteRepository.Add(curPlan);
//                uow.SaveChanges();
//                //Add a processnodetemplate to processtemplate 
//                var curSubroute = FixtureData.TestSubrouteDO1();
//                curSubroute.ParentTemplateId = curPlan.Id;
//
//                uow.SubrouteRepository.Add(curSubroute);
//                uow.SaveChanges();
//                
//                var actionList = FixtureData.TestEmptyActionList();
//                actionList.Id = 1;
//                actionList.ActionListType = 1;
//                actionList.SubrouteID = curSubroute.Id;
//
//                uow.ActionListRepository.Add(actionList);
//                uow.SaveChanges();
//            }
//        }

        private void CreateActionTemplate()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ActivityTemplateRepository.Add(FixtureData.TestActivityTemplateDO1());
                uow.SaveChanges();
            }
        }

        /// <summary>
        /// Creates a new Action with the given action ID
        /// </summary>
        private ActivityDTO CreateActionWithId(Guid actionId)
        {
            return new ActivityDTO
            {
                Id = actionId,
                Name = "WriteToAzureSql",
                CrateStorage = new CrateStorageDTO(),
                ActivityTemplate = FixtureData.TestActionTemplateDTOV2()
                //,ActionTemplate = FixtureData.TestActivityTemplateDO2()
            };
        }

//        private ActivityDO CreateActionWithV2ActionTemplate(IUnitOfWork uow)
//        {
//
//            var curActionTemplate = FixtureData.TestActivityTemplateV2();
//            uow.ActivityTemplateRepository.Add(curActionTemplate);
//
//            var curAction = FixtureData.TestActivity1();
//            curAction.ActivityTemplateId = curActionTemplate.Id;
//            curAction.ActivityTemplate = curActionTemplate;
//            uow.ActivityRepository.Add(curAction);
//
//            return curAction;
//        }


     

        [Test, Ignore]

        public async void ActionController_GetConfigurationSettings_ValidActionDesignDTO()
        {
            var controller = new ActionsController();
            ActivityDTO actionDesignDTO = CreateActionWithId(FixtureData.GetTestGuidById(2));
            actionDesignDTO.ActivityTemplate = FixtureData.TestActionTemplateDTOV2();
            var actionResult = await controller.Configure(actionDesignDTO);

            var okResult = actionResult as OkNegotiatedContentResult<ActivityDO>;

            Assert.IsNotNull(okResult);
            Assert.IsNotNull(okResult.Content);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ApplicationException), ExpectedMessage = "Could not find Action.")]
        public async void ActionController_GetConfigurationSettings_IdIsMissing()
        {
            var controller = new ActionsController();
            ActivityDTO actionDesignDTO = CreateActionWithId(FixtureData.GetTestGuidById(2));
            actionDesignDTO.Id = Guid.Empty;
            var actionResult = await controller.Configure(actionDesignDTO);

            var okResult = actionResult as OkNegotiatedContentResult<ActivityDO>;

            Assert.IsNotNull(okResult);
            Assert.IsNotNull(okResult.Content);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(KeyNotFoundException))]
        public async void ActionController_GetConfigurationSettings_ActionTemplateIdIsMissing()
        {
            var controller = new ActionsController();
            ActivityDTO actionDesignDTO = CreateActionWithId(FixtureData.GetTestGuidById(2));
            var actionResult = await controller.Configure(actionDesignDTO);

            var okResult = actionResult as OkNegotiatedContentResult<ActivityDO>;

            Assert.IsNotNull(okResult);
            Assert.IsNotNull(okResult.Content);
        }
    }
}
