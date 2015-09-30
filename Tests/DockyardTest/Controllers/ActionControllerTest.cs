using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Core.Services;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Web.Controllers;
using Web.ViewModels;
using Moq;
using System;
using Core.Interfaces;
using System.Web.Http.Results;
using AutoMapper;

namespace DockyardTest.Controllers
{
    [TestFixture]
    public class ActionControllerTest : BaseTest
    {

        private IAction _action;

        public ActionControllerTest()
        {

        }
        public override void SetUp()
        {
            base.SetUp();
            _action = ObjectFactory.GetInstance<IAction>();
            CreateEmptyActionList();
            CreateActionTemplate();
        }


        [Test,Ignore]
        [Category("ActionController.Save")]
        public void ActionController_Save_WithEmptyActions_NewActionShouldBeCreated()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Arrange is done with empty action list

                //Act
                var actualAction = CreateActionWithId(1);

                var controller = new ActionController();
                controller.Save(actualAction);

                //Assert
                Assert.IsNotNull(uow.ActionRepository);
                Assert.IsTrue(uow.ActionRepository.GetAll().Count() == 1);

                var expectedAction = uow.ActionRepository.GetByKey(actualAction.Id);
                Assert.IsNotNull(expectedAction);
                Assert.AreEqual(actualAction.Name, expectedAction.Name);
            }
        }

        [Test,Ignore]
        [Category("ActionController.Save")]
        public void ActionController_Save_WithActionNotExisting_NewActionShouldBeCreated()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Arrange
                //Add one test action
                var action = FixtureData.TestAction1();
                uow.ActionRepository.Add(action);
                uow.SaveChanges();

                //Act
                var actualAction = CreateActionWithId(2);

                var controller = new ActionController();
                controller.Save(actualAction);

                //Assert
                Assert.IsNotNull(uow.ActionRepository);
                Assert.IsTrue(uow.ActionRepository.GetAll().Count() == 2);

                //Still there is only one action as the update happened.
                var expectedAction = uow.ActionRepository.GetByKey(actualAction.Id);
                Assert.IsNotNull(expectedAction);
                Assert.AreEqual(actualAction.Name, expectedAction.Name);
            }
        }

        [Test]
        [Category("ActionController.Save")]
        public void ActionController_Save_WithActionExists_ExistingActionShouldBeUpdated()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Arrange
                //Add one test action
                var action = FixtureData.TestAction1();
                uow.ActionRepository.Add(action);
                uow.SaveChanges();

                //Act
                var actualAction = CreateActionWithId(1);

                var controller = new ActionController();
                controller.Save(actualAction);

                //Assert
                Assert.IsNotNull(uow.ActionRepository);
                Assert.IsTrue(uow.ActionRepository.GetAll().Count() == 1);

                //Still there is only one action as the update happened.
                var expectedAction = uow.ActionRepository.GetByKey(actualAction.Id);
                Assert.IsNotNull(expectedAction);
                Assert.AreEqual(actualAction.Name, expectedAction.Name);
            }
        }

        [Test, Ignore("Vas Ignored as part of V2 Changes")]
        [Category("ActionController.GetConfigurationSettings")]
        public void ActionController_GetConfigurationSettings_CanGetCorrectJson()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curActionDO = FixtureData.TestAction22();

                var expectedResult = FixtureData.TestConfigurationSettings();
                //CrateStorageDTO result = _action.Configure(curActionDO);
                //Assert.GreaterOrEqual(1, result.CrateDTO.Count);
            }
        }

        [Test, Ignore]
        [Category("ActionController.GetConfigurationSettings")]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public void ActionController_NULL_ActionTemplate()
        {
            var curAction = new ActionController();
            var actionDO = curAction.Configure(CreateActionWithId(2));
            Assert.IsNotNull(actionDO);
        }

        [Test]
        [Category("ActionController.Configure")]
        [Ignore("The real server is not in execution in AppVeyor. Remove these tests once Jasmine Front End integration tests are added.")]
        public async void ActionController_Configure_WithoutConnectionString_ShouldReturnOneEmptyConnectionString()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Arrange
                //remvoe existing action templates
                uow.ActivityTemplateRepository.Remove(uow.ActivityTemplateRepository.GetByKey(1));
                uow.SaveChanges();

                //create action
                var curAction = CreateActionWithV2ActionTemplate(uow);
                curAction.CrateStorage = JsonConvert.SerializeObject(FixtureData.TestConfigurationStore());
                uow.SaveChanges();

                var curActionDesignDO = Mapper.Map<ActionDTO>(curAction);
                //Act
                var result = await
                    new ActionController(_action).Configure(curActionDesignDO) as
                        OkNegotiatedContentResult<string>;

                CrateStorageDTO resultantCrateStorageDto =
                    JsonConvert.DeserializeObject<CrateStorageDTO>(result.Content);

                //Assert
                Assert.IsNotNull(result, "Configure POST reqeust is failed");
                Assert.IsNotNull(resultantCrateStorageDto, "Configure returns no Configuration Store");
                Assert.IsTrue(resultantCrateStorageDto.CrateDTO.Count == 1, "Configure is not assuming this is the first request from the client");
                //different V2 format
                //Assert.AreEqual("connection_string", resultantCrateStorageDto.Fields[0].Name, "Configure does not return one connection string with empty value");
                //Assert.IsEmpty(resultantCrateStorageDto.Fields[0].Value, "Configure returned some connectoin string when the first request made");
                
                ////There should be no data fields as this is the first request from the client
                //Assert.IsTrue(resultantCrateStorageDto.DataFields.Count == 0, "Configure did not assume this is the first call from the client");
            }
        }

        [Test]
        [Category("ActionController.Configure")]
        [Ignore("The real server is not in execution in AppVeyor. Remove these tests once Jasmine Front End integration tests are added.")]
        public async void ActionController_Configure_WithConnectionString_ShouldReturnDataFields()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Arrange
                //remvoe existing action templates
                uow.ActivityTemplateRepository.Remove(uow.ActivityTemplateRepository.GetByKey(1));
                uow.SaveChanges();

                //create action
                var curAction = CreateActionWithV2ActionTemplate(uow);
                var configurationStore = FixtureData.TestConfigurationStore();
                //different V2 format
                //configurationStore.Fields[0].Value = "Data Source=s79ifqsqga.database.windows.net;database=demodb_health;User ID=alexeddodb;Password=Thales89;";
                curAction.CrateStorage = JsonConvert.SerializeObject(configurationStore);
                uow.SaveChanges();
                var curActionDesignDO = Mapper.Map<ActionDTO>(curAction);
                //Act
                var result = await
                    new ActionController(_action).Configure(curActionDesignDO) as
                        OkNegotiatedContentResult<string>;

                CrateStorageDTO resultantCrateStorageDto =
                    JsonConvert.DeserializeObject<CrateStorageDTO>(result.Content);

                //Assert
                Assert.IsNotNull(result, "Configure POST reqeust is failed");
                Assert.IsNotNull(resultantCrateStorageDto, "Configure returns no Configuration Store");
                Assert.IsTrue(resultantCrateStorageDto.CrateDTO.Count == 3, "Configure returned invalid data fields");
            }
        }

        [Test]
        [Category("ActionController.Configure")]
        [Ignore("The real server is not in execution in AppVeyor. Remove these tests once Jasmine Front End integration tests are added.")]
        public async void ActionController_Configure_WithConnectionStringAndDataFields_ShouldReturnUpdatedDataFields()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Arrange
                //remvoe existing action templates
                uow.ActivityTemplateRepository.Remove(uow.ActivityTemplateRepository.GetByKey(1));
                uow.SaveChanges();

                //create action
                var curAction = CreateActionWithV2ActionTemplate(uow);
                var configurationStore = FixtureData.TestConfigurationStore();
                //V2 changes
                //configurationStore.Fields[0].Value = "Data Source=s79ifqsqga.database.windows.net;database=demodb_health;User ID=alexeddodb;Password=Thales89;";
                //configurationStore.DataFields.Add("something");
                //configurationStore.DataFields.Add("Wrong");
                //configurationStore.DataFields.Add("data fields");
                //configurationStore.DataFields.Add("data fields");
                curAction.CrateStorage = JsonConvert.SerializeObject(configurationStore);
                uow.SaveChanges();
                var curActionDesignDO = Mapper.Map<ActionDTO>(curAction);
                //Act
                var result = await
                    new ActionController(_action).Configure(curActionDesignDO) as
                        OkNegotiatedContentResult<string>;

                CrateStorageDTO resultantCrateStorageDto =
                    JsonConvert.DeserializeObject<CrateStorageDTO>(result.Content);

                //Assert
                Assert.IsNotNull(result, "Configure POST reqeust is failed");
                Assert.IsNotNull(resultantCrateStorageDto, "Configure returns no Configuration Store");
                //V2 changes
                //Assert.IsTrue(resultantCrateStorageDto.DataFields.Count != 4, "Since we already had 4 invalid data fields, the number of data fields should not be 4 now.");
                //Assert.IsTrue(resultantCrateStorageDto.DataFields.Count == 3, "The new data field should be 3 data fields as with the update one.");
            }
        }

        [Test]
        [Category("Controllers.ActionController")]
        public void ActionController_Delete()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Mock<IAction> actionMock = new Mock<IAction>();
                actionMock.Setup(a => a.Delete(It.IsAny<int>()));

                ActionDO actionDO = new FixtureData(uow).TestAction3();
                var controller = new ActionController(actionMock.Object);
                controller.Delete(actionDO.Id);
                actionMock.Verify(a => a.Delete(actionDO.Id));
            }
        }

        [Test]
        [Category("Controllers.ActionController")]
        public void ActionController_Get()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Mock<IAction> actionMock = new Mock<IAction>();
                actionMock.Setup(a => a.GetById(It.IsAny<int>()));

                ActionDO actionDO = new FixtureData(uow).TestAction3();
                var controller = new ActionController(actionMock.Object);
                controller.Get(actionDO.Id);
                actionMock.Verify(a => a.GetById(actionDO.Id));
            }
        }

        /// <summary>
        /// Creates one empty action list
        /// </summary>
        private void CreateEmptyActionList()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Add a template
                var curProcessTemplate = FixtureData.TestProcessTemplate1();
                uow.ProcessTemplateRepository.Add(curProcessTemplate);
                uow.SaveChanges();

                var curProcessNodeTemplate = FixtureData.TestProcessNodeTemplateDO1();
                curProcessNodeTemplate.ParentTemplateId = curProcessTemplate.Id;

                uow.ProcessNodeTemplateRepository.Add(curProcessNodeTemplate);
                uow.SaveChanges();

                var actionList = FixtureData.TestEmptyActionList();
                actionList.Id = 1;
                actionList.ActionListType = 1;
                actionList.ProcessNodeTemplateID = curProcessNodeTemplate.Id;

                uow.ActionListRepository.Add(actionList);
                uow.SaveChanges();
            }
        }

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
        private ActionDTO CreateActionWithId(int actionId)
        {
            return new ActionDTO
            {
                Id = actionId,
                Name = "WriteToAzureSql",
                ActionListId = 1,
                CrateStorage = new CrateStorageDTO(),
                FieldMappingSettings = new FieldMappingSettingsDTO(),
                ActivityTemplateId = 1,
                ActivityTemplate = FixtureData.TestActionTemplateDTOV2()
                //,ActionTemplate = FixtureData.TestActivityTemplateDO2()
            };
        }

        private ActionDO CreateActionWithV2ActionTemplate(IUnitOfWork uow)
        {

            var curActionTemplate = FixtureData.TestActivityTemplateV2();
            uow.ActivityTemplateRepository.Add(curActionTemplate);

            var curAction = FixtureData.TestAction1();
            curAction.ActivityTemplateId = curActionTemplate.Id;
            curAction.ActivityTemplate = curActionTemplate;
            uow.ActionRepository.Add(curAction);

            return curAction;
        }


     

        [Test, Ignore]
        [Category("ActionController")]
        public async void ActionController_GetConfigurationSettings_ValidActionDesignDTO()
        {
            var controller = new ActionController();
            ActionDTO actionDesignDTO = CreateActionWithId(2);
            actionDesignDTO.ActivityTemplate = FixtureData.TestActionTemplateDTOV2();
            var actionResult = await controller.Configure(actionDesignDTO);

            var okResult = actionResult as OkNegotiatedContentResult<ActionDO>;

            Assert.IsNotNull(okResult);
            Assert.IsNotNull(okResult.Content);
        }

        [Test, Ignore("Vas Ignored as part of V2 Changes")]
        [Category("ActionController")]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public async void ActionController_GetConfigurationSettings_IdIsMissing()
        {
            var controller = new ActionController();
            ActionDTO actionDesignDTO = CreateActionWithId(2);
            actionDesignDTO.Id = 0;
            var actionResult = await controller.Configure(actionDesignDTO);

            var okResult = actionResult as OkNegotiatedContentResult<ActionDO>;

            Assert.IsNotNull(okResult);
            Assert.IsNotNull(okResult.Content);
        }

        [Test, Ignore("Vas Ignored as part of V2 Changes")]
        [Category("ActionController")]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public async void ActionController_GetConfigurationSettings_ActionTemplateIdIsMissing()
        {
            var controller = new ActionController();
            ActionDTO actionDesignDTO = CreateActionWithId(2);
            actionDesignDTO.ActivityTemplateId = 0;
            var actionResult = await controller.Configure(actionDesignDTO);

            var okResult = actionResult as OkNegotiatedContentResult<ActionDO>;

            Assert.IsNotNull(okResult);
            Assert.IsNotNull(okResult.Content);
        }
    }
}
