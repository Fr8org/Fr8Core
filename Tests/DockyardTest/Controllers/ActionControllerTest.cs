using System.Linq;
using Core.Services;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
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

namespace DockyardTest.Controllers
{
    [TestFixture]
    public class ActionControllerTest : BaseTest
    {
        public override void SetUp()
        {
            base.SetUp();
            CreateEmptyActionList();
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
                Assert.AreEqual(actualAction.UserLabel, expectedAction.UserLabel);
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
                Assert.AreEqual(actualAction.UserLabel, expectedAction.UserLabel);
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
                Assert.AreEqual(actualAction.UserLabel, expectedAction.UserLabel);
            }
        }

        [Test]
        [Category("ActionController.GetConfigurationSettings")]
        public void ActionController_GetConfigurationSettings_CanGetCorrectJson()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curActionRegistration = FixtureData.TestActionRegistrationDO1();
                var _service = new Core.Services.Action();
                string curJsonResult = "{\"configurationSettings\":[{\"textField\": {\"name\": \"connection_string\",\"required\":true,\"value\":\"\",\"fieldLabel\":\"SQL Connection String\",}}]}";
                Assert.AreEqual(_service.GetConfigurationSettings(curActionRegistration).ConfigurationSettings, curJsonResult);
            }
        }

        [Test]
        [Category("ActionController.GetConfigurationSettings")]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public void ActionController_NULL_ActionRegistration()
        {
            var curAction = new ActionController();
            Assert.IsNotNull(curAction.GetConfigurationSettings(1));
        }

        [Test]
        [Category("BasePluginRegistration.AssembleName")]
        public void BasePluginRegistration_AssembleName__CanConcatinateParentPluginRegistrationAndVersion()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curActionRegistration = FixtureData.TestActionRegistrationDO1();
                var _pluginRegistration = ObjectFactory.GetInstance<IPluginRegistration>();
                string assembeledName = "Core.PluginRegistrations.AzureSqlServerPluginRegistration_v1";
                Assert.AreEqual(_pluginRegistration.AssembleName(curActionRegistration), assembeledName);
            }
        }

        [Test]
        [Category("BasePluginRegistration.CallPluginRegistrationByString")]
        public void BasePluginRegistration_CallPluginRegistrationByString__ShouldReturnConfigurationSettingsJson()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curActionRegistration = FixtureData.TestActionRegistrationDO1();
                var _pluginRegistration = ObjectFactory.GetInstance<IPluginRegistration>();
                string curJsonResult = "{\"configurationSettings\":[{\"textField\": {\"name\": \"connection_string\",\"required\":true,\"value\":\"\",\"fieldLabel\":\"SQL Connection String\",}}]}";
                Assert.AreEqual(_pluginRegistration.CallPluginRegistrationByString("Core.PluginRegistrations.AzureSqlServerPluginRegistration_v1", "GetConfigurationSettings", curActionRegistration), curJsonResult);
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
                var template = FixtureData.TestTemplate1();
                var templates = uow.Db.Set<TemplateDO>();
                templates.Add(template);
                uow.Db.SaveChanges();

                var actionList = FixtureData.TestEmptyActionList();
                actionList.Id = 1;
                actionList.ActionListType = 1;

                uow.ActionListRepository.Add(actionList);
                uow.SaveChanges();
            }
        }

        /// <summary>
        /// Creates a new Action with the given actiond ID
        /// </summary>
        private ActionDTO CreateActionWithId(int actionId)
        {
            return new ActionDTO
            {
                Id = actionId,
                UserLabel = "AzureSqlAction",
                ActionType = "WriteToAzureSql",
                ActionListId = 1,
                ConfigurationSettings = "JSON Config Settings",
                FieldMappingSettings = "JSON Field Mapping Settings",
                ParentPluginRegistration = "AzureSql"
            };
        }


        [Test]
        public async  void Can_Create_Instance()
        {
            //Arrange 
            string pluginName =
                "Core.PluginRegistrations.AzureSqlServerPluginRegistration_v1, Core";
            var cntroller = new ActionController();
            //cntroller.GetFieldMappingTargets(new ActionDTO() { ParentPluginRegistration = pluginName });
            
            var task = cntroller.GetFieldMappingTargets(new ActionDTO() { ParentPluginRegistration = pluginName });
            await task;
        }
    }
}
