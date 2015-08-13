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
        [Category("Controllers.ActionController.Save")]
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
        [Category("Controllers.ActionController.Save")]
        public void ActionController_Save_WithActionNotExisting_NewActionShouldBeCreated()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Arrange
                //Add one test action
                var action = new FixtureData(uow).TestAction1();
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
        [Category("Controllers.ActionController.Save")]
        public void ActionController_Save_WithActionExists_ExistingActionShouldBeUpdated()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Arrange
                //Add one test action
                var action = new FixtureData(uow).TestAction1();
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

        /// <summary>
        /// Creates one empty action list
        /// </summary>
        private void CreateEmptyActionList()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);

                //Add a template
                var template = fixture.TestTemplate1();
                var templates = uow.Db.Set<TemplateDO>();
                templates.Add(template);
                uow.Db.SaveChanges();

                var actionList = new FixtureData(uow).TestEmptyActionList();
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
    }
}
