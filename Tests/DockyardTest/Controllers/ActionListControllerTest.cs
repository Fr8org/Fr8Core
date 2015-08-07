using System;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Web.Controllers;
using Web.ViewModels;

namespace DockyardTest.Controllers
{
    [TestFixture]
    public class ActionListControllerTest : BaseTest
    {
        public override void SetUp()
        {
            base.SetUp();
           
        }

        [Test]
        [Category("Controllers.ActionListController.AddAction")]
        public void ActionListController_Add_ActionDO_To_ActionListDO_With_Last_Position()
        {
            CreateActionList();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var controller = new ActionListController();
                ActionDO actionDO = CreateActionDO();
                // call AddAction(actionDO, "last") -
                controller.AddAction(actionDO, "last");
                //Assert
                Assert.IsNotNull(uow.ActionRepository.GetByKey(10));
                Assert.AreEqual(uow.ActionRepository.GetByKey(10).Ordering, 3);
            }
        }

        [Test]
        [Category("Controllers.ActionListController.AddAction")]
        public void ActionListController_Add_ActionDO_To_ActionListDO_With_Default_Position()
        {
            CreateActionList();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var controller = new ActionListController();
                ActionDO actionDO = CreateActionDO();
                // call AddAction(actionDO, "last") -
                controller.AddAction(actionDO, "");
                //Assert
                Assert.IsNotNull(uow.ActionRepository.GetByKey(10));
                Assert.AreEqual(uow.ActionRepository.GetByKey(10).Ordering, 0);
            }
        }

        [Test]
        [Category("Controllers.ActionListController.AddAction")]
        public void ActionListController_Set_LowestPostitoned_ActionDO_To_CurrentAtion_Of_ActionListDO()
        {
            CreateActionListWithNullCurrentAction();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var controller = new ActionListController();
                ActionDO actionDO = CreateActionDO();
                // call AddAction(actionDO, "last") -
                controller.AddAction(actionDO, "last");
                //Assert
                Assert.IsNotNull(uow.ActionListRepository.GetByKey(1));
                Assert.AreEqual(uow.ActionListRepository.GetByKey(1).CurrentAction.Ordering, 1);
            }
        }

        [Test]
        [Category("Controllers.ActionListController.Process")]
        [ExpectedException(ExpectedException= typeof(ArgumentNullException))]
        public void ActionListController_NULL_CurrentAtion_Of_ActionListDO()
        {
            CreateActionListWithNullCurrentAction();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var controller = new ActionListController();
                Assert.IsNotNull(uow.ActionListRepository.GetByKey(1));
                controller.Process(uow.ActionListRepository.GetByKey(1));
            }
        }

        #region Private methods
        /// <summary>
        /// Creates a new Action with the given actiond ID
        /// </summary>
        private ActionDO CreateActionDO()
        {
            return new ActionDO
            {
                Id = 10,
                UserLabel = "AzureSqlAction",
                ActionType = "WriteToAzureSql",
                ActionListId = 1,
                ConfigurationSettings = "JSON Config Settings",
                FieldMappingSettings = "JSON Field Mapping Settings",
                ParentPluginRegistration = "AzureSql",
                Ordering = 1
            };
        }

        private void CreateActionList()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);

                //Add a template
                var template = fixture.TestTemplate1();
                var templates = uow.Db.Set<TemplateDO>();
                templates.Add(template);
                uow.Db.SaveChanges();

                var actionList = new FixtureData(uow).TestActionList();

                uow.ActionListRepository.Add(actionList);
                uow.SaveChanges();
            }
        }

        private void CreateActionListWithNullCurrentAction()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);

                //Add a template
                var template = fixture.TestTemplate1();
                var templates = uow.Db.Set<TemplateDO>();
                templates.Add(template);
                uow.Db.SaveChanges();

                var actionList = new FixtureData(uow).TestActionList();
                actionList.CurrentAction = null;
                uow.ActionListRepository.Add(actionList);
                uow.SaveChanges();
            }
        }
        #endregion
    }
}
