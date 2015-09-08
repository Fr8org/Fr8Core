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
using Core.PluginRegistrations;
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


        [Test]
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

        [Test]
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
                string curJsonResult = _action.GetConfigurationSettings(curActionDO);
                ConfigurationSettingsDTO result = Newtonsoft.Json.JsonConvert.DeserializeObject<ConfigurationSettingsDTO>(curJsonResult);
                Assert.AreEqual(1, result.Fields.Count);
                Assert.AreEqual(expectedResult.Fields[0].FieldLabel, result.Fields[0].FieldLabel);
                Assert.AreEqual(expectedResult.Fields[0].Type, result.Fields[0].Type);
                Assert.AreEqual(expectedResult.Fields[0].Name, result.Fields[0].Name);
                Assert.AreEqual(expectedResult.Fields[0].Required, result.Fields[0].Required);
            }
        }

        [Test, Ignore]
        [Category("ActionController.GetConfigurationSettings")]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public void ActionController_NULL_ActionTemplate()
        {
            var curAction = new ActionController();
            var actionDO = curAction.GetConfigurationSettings(CreateActionWithId(2));
            Assert.IsNotNull(actionDO);
        }

        [Test]
        [Category("ActionController.Configure")]
        [Ignore("The real server is not in execution in AppVeyor. Remove these tests once Jasmine Front End integration tests are added.")]
        public void ActionController_Configure_WithoutConnectionString_ShouldReturnOneEmptyConnectionString()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Arrange
                //remvoe existing action templates
                uow.ActionTemplateRepository.Remove(uow.ActionTemplateRepository.GetByKey(1));
                uow.SaveChanges();

                //create action
                var curAction = CreateActionWithV2ActionTemplate(uow);
                curAction.ConfigurationStore = JsonConvert.SerializeObject(FixtureData.TestConfigurationStore());
                uow.SaveChanges();

                var curActionDesignDO = Mapper.Map<ActionDesignDTO>(curAction);
                //Act
                var result =
                    new ActionController(_action).GetConfigurationSettings(curActionDesignDO) as
                        OkNegotiatedContentResult<string>;

                ConfigurationSettingsDTO resultantConfigurationSettingsDto =
                    JsonConvert.DeserializeObject<ConfigurationSettingsDTO>(result.Content);

                //Assert
                Assert.IsNotNull(result, "Configure POST reqeust is failed");
                Assert.IsNotNull(resultantConfigurationSettingsDto, "Configure returns no Configuration Store");
                Assert.IsTrue(resultantConfigurationSettingsDto.Fields.Count == 1, "Configure is not assuming this is the first request from the client");
                Assert.AreEqual("connection_string", resultantConfigurationSettingsDto.Fields[0].Name, "Configure does not return one connection string with empty value");
                Assert.IsEmpty(resultantConfigurationSettingsDto.Fields[0].Value, "Configure returned some connectoin string when the first request made");
                
                //There should be no data fields as this is the first request from the client
                Assert.IsTrue(resultantConfigurationSettingsDto.DataFields.Count == 0, "Configure did not assume this is the first call from the client");
            }
        }

        [Test]
        [Category("ActionController.Configure")]
        [Ignore("The real server is not in execution in AppVeyor. Remove these tests once Jasmine Front End integration tests are added.")]
        public void ActionController_Configure_WithConnectionString_ShouldReturnDataFields()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Arrange
                //remvoe existing action templates
                uow.ActionTemplateRepository.Remove(uow.ActionTemplateRepository.GetByKey(1));
                uow.SaveChanges();

                //create action
                var curAction = CreateActionWithV2ActionTemplate(uow);
                var configurationStore = FixtureData.TestConfigurationStore();
                configurationStore.Fields[0].Value = "Data Source=s79ifqsqga.database.windows.net;database=demodb_health;User ID=alexeddodb;Password=Thales89;";
                curAction.ConfigurationStore = JsonConvert.SerializeObject(configurationStore);
                uow.SaveChanges();
                var curActionDesignDO = Mapper.Map<ActionDesignDTO>(curAction);
                //Act
                var result =
                    new ActionController(_action).GetConfigurationSettings(curActionDesignDO) as
                        OkNegotiatedContentResult<string>;

                ConfigurationSettingsDTO resultantConfigurationSettingsDto =
                    JsonConvert.DeserializeObject<ConfigurationSettingsDTO>(result.Content);

                //Assert
                Assert.IsNotNull(result, "Configure POST reqeust is failed");
                Assert.IsNotNull(resultantConfigurationSettingsDto, "Configure returns no Configuration Store");
                Assert.IsTrue(resultantConfigurationSettingsDto.DataFields.Count == 3, "Configure returned invalid data fields");
            }
        }

        [Test]
        [Category("ActionController.Configure")]
        [Ignore("The real server is not in execution in AppVeyor. Remove these tests once Jasmine Front End integration tests are added.")]
        public void ActionController_Configure_WithConnectionStringAndDataFields_ShouldReturnUpdatedDataFields()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Arrange
                //remvoe existing action templates
                uow.ActionTemplateRepository.Remove(uow.ActionTemplateRepository.GetByKey(1));
                uow.SaveChanges();

                //create action
                var curAction = CreateActionWithV2ActionTemplate(uow);
                var configurationStore = FixtureData.TestConfigurationStore();
                configurationStore.Fields[0].Value = "Data Source=s79ifqsqga.database.windows.net;database=demodb_health;User ID=alexeddodb;Password=Thales89;";
                configurationStore.DataFields.Add("something");
                configurationStore.DataFields.Add("Wrong");
                configurationStore.DataFields.Add("data fields");
                configurationStore.DataFields.Add("data fields");
                curAction.ConfigurationStore = JsonConvert.SerializeObject(configurationStore);
                uow.SaveChanges();
                var curActionDesignDO = Mapper.Map<ActionDesignDTO>(curAction);
                //Act
                var result =
                    new ActionController(_action).GetConfigurationSettings(curActionDesignDO) as
                        OkNegotiatedContentResult<string>;

                ConfigurationSettingsDTO resultantConfigurationSettingsDto =
                    JsonConvert.DeserializeObject<ConfigurationSettingsDTO>(result.Content);

                //Assert
                Assert.IsNotNull(result, "Configure POST reqeust is failed");
                Assert.IsNotNull(resultantConfigurationSettingsDto, "Configure returns no Configuration Store");
                Assert.IsTrue(resultantConfigurationSettingsDto.DataFields.Count != 4, "Since we already had 4 invalid data fields, the number of data fields should not be 4 now.");
                Assert.IsTrue(resultantConfigurationSettingsDto.DataFields.Count == 3, "The new data field should be 3 data fields as with the update one.");
            }
        }

        [Test]
        [Category("BasePluginRegistration.AssembleName")]
        public void BasePluginRegistration_AssembleName__CanConcatinateParentPluginRegistrationAndVersion()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curActionTemplate = FixtureData.TestActionTemplateDO1();
                var _pluginRegistration = ObjectFactory.GetInstance<IPluginRegistration>();
                string assembeledName = "Core.PluginRegistrations.AzureSqlServerPluginRegistration_v1";
                Assert.AreEqual(_pluginRegistration.AssembleName(curActionTemplate), assembeledName);
            }
        }

        [Test]
        [Category("BasePluginRegistration.CallPluginRegistrationByString")]
        public void BasePluginRegistration_CallPluginRegistrationByString__ShouldReturnConfigurationSettingsJson()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curActionTemplate = FixtureData.TestActionTemplateDO1();
                var _pluginRegistration = ObjectFactory.GetInstance<IPluginRegistration>();
                var expectedResult = FixtureData.TestConfigurationSettings();
                string curJsonResult = _pluginRegistration.CallPluginRegistrationByString("Core.PluginRegistrations.AzureSqlServerPluginRegistration_v1", "GetConfigurationSettings", FixtureData.TestAction1());
                ConfigurationSettingsDTO result = Newtonsoft.Json.JsonConvert.DeserializeObject<ConfigurationSettingsDTO>(curJsonResult);
                Assert.AreEqual(1, result.Fields.Count);
                Assert.AreEqual(expectedResult.Fields[0].FieldLabel, result.Fields[0].FieldLabel);
                Assert.AreEqual(expectedResult.Fields[0].Type, result.Fields[0].Type);
                Assert.AreEqual(expectedResult.Fields[0].Name, result.Fields[0].Name);
                Assert.AreEqual(expectedResult.Fields[0].Required, result.Fields[0].Required);
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
                var curProcessNodeTemplate = FixtureData.TestProcessNodeTemplateDO1();
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
                uow.ActionTemplateRepository.Add(FixtureData.TestActionTemplateDO1());
                uow.SaveChanges();
            }
        }

        /// <summary>
        /// Creates a new Action with the given action ID
        /// </summary>
        private ActionDesignDTO CreateActionWithId(int actionId)
        {
            return new ActionDesignDTO
            {
                Id = actionId,
                Name = "WriteToAzureSql",
                ActionListId = 1,
                ConfigurationStore = new ConfigurationSettingsDTO(),
                FieldMappingSettings = new FieldMappingSettingsDTO(),
                ActionTemplateId = 1,
                ActionTemplate = FixtureData.TestActionTemplateDTOV2()
                //,ActionTemplate = FixtureData.TestActionTemplateDO2()
            };
        }

        private ActionDO CreateActionWithV2ActionTemplate(IUnitOfWork uow)
        {

            var curActionTemplate = FixtureData.TestActionTemplateV2();
            uow.ActionTemplateRepository.Add(curActionTemplate);

            var curAction = FixtureData.TestAction1();
            curAction.ActionTemplateId = curActionTemplate.Id;
            curAction.ActionTemplate = curActionTemplate;
            uow.ActionRepository.Add(curAction);

            return curAction;
        }


        [Test]
        [Ignore("Vas, Ignored as part of V2 changes")]
        // To run and pass this test 
        // pluginAzureSqlServer should be running 
        // as of now the endpoint it connects to is hardcoded to be "http://localhost:46281/plugin_azure_sql_server"
        // make sure that the endpoint is running 
        // in azure db you need a db demodb_health
        public async void Can_Get_FieldMappingTargets()
        {

            //Arrange 
            string pluginName =
                "Core.PluginRegistrations.AzureSqlServerPluginRegistration_v1, Core";
            string dataSource =
                "Data Source=s79ifqsqga.database.windows.net;database=demodb_health;User ID=alexeddodb;Password=Thales89;";
            var cntroller = new ActionController();
            //cntroller.GetFieldMappingTargets(new ActionDTO() { ParentPluginRegistration = pluginName });

            var task = cntroller.GetFieldMappingTargets(new ActionDesignDTO()
            {
                ConfigurationStore = Newtonsoft.Json.JsonConvert.DeserializeObject<ConfigurationSettingsDTO>(
                    "{\"connection_string\":\"" + dataSource + "\"}")
            });

            //await task;
            //Assert.NotNull(task.Result);
            //Assert.Greater(task.Result.Count(), 0);
            //task.Result.ToList().ForEach(Console.WriteLine);
        }

        [Test, Ignore]
        [Category("ActionController")]
        public void ActionController_GetConfigurationSettings_ValidActionDesignDTO()
        {
            var controller = new ActionController();
            ActionDesignDTO actionDesignDTO = CreateActionWithId(2);
            actionDesignDTO.ActionTemplate = FixtureData.TestActionTemplateDTOV2();
            var actionResult = controller.GetConfigurationSettings(actionDesignDTO);

            var okResult = actionResult as OkNegotiatedContentResult<ActionDO>;

            Assert.IsNotNull(okResult);
            Assert.IsNotNull(okResult.Content);
        }

        [Test, Ignore("Vas Ignored as part of V2 Changes")]
        [Category("ActionController")]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public void ActionController_GetConfigurationSettings_IdIsMissing()
        {
            var controller = new ActionController();
            ActionDesignDTO actionDesignDTO = CreateActionWithId(2);
            actionDesignDTO.Id = 0;
            var actionResult = controller.GetConfigurationSettings(actionDesignDTO);

            var okResult = actionResult as OkNegotiatedContentResult<ActionDO>;

            Assert.IsNotNull(okResult);
            Assert.IsNotNull(okResult.Content);
        }

        [Test, Ignore("Vas Ignored as part of V2 Changes")]
        [Category("ActionController")]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public void ActionController_GetConfigurationSettings_ActionTemplateIdIsMissing()
        {
            var controller = new ActionController();
            ActionDesignDTO actionDesignDTO = CreateActionWithId(2);
            actionDesignDTO.ActionTemplateId = 0;
            var actionResult = controller.GetConfigurationSettings(actionDesignDTO);

            var okResult = actionResult as OkNegotiatedContentResult<ActionDO>;

            Assert.IsNotNull(okResult);
            Assert.IsNotNull(okResult.Content);
        }
    }
}
