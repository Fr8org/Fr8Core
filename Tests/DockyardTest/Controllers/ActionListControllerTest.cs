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
using Core.Services;

namespace DockyardTest.Controllers
{
    [TestFixture]
    public class ActionListControllerTest : BaseTest
    {

        public override void SetUp()
        {
            base.SetUp();
            InitializeActionList();
        }

        [Test]
        [Category("ActionListController.AddAction")]
        public void ActionListController_CanAddActionDOInLastPosition()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curService = new ActionList();
                ActionDO actionDO = CreateActionDO();
                curService.AddAction(actionDO, "last");
                Assert.IsNotNull(uow.ActionRepository.GetByKey(10));
                Assert.AreEqual(uow.ActionRepository.GetByKey(10).Ordering, 3);
            }
        }

        [Test]
        [Category("ActionListController.AddAction")]
        [ExpectedException(ExpectedException = typeof(NotSupportedException))]
        public void ActionListController_CanAddActionDOInPositionOtherThanLast()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curService = new ActionList();
                ActionDO actionDO = CreateActionDO();
                curService.AddAction(actionDO, "first");
            }
        }

        [Test]
        [Category("ActionListController.AddAction")]
        public void ActionListController_Set_LowestPostitoned_ActionDO_To_CurrentAtion_Of_ActionListDO()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curService = new ActionList();
                ActionDO actionDO = CreateActionDO();
                curService.AddAction(actionDO, "last");
                Assert.IsNotNull(uow.ActionListRepository.GetByKey(1));
                Assert.AreEqual(uow.ActionListRepository.GetByKey(1).CurrentAction.Ordering, 1);
            }
        }

        [Test]
        [Category("ActionListController.Process")]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public void ActionListController_NULL_CurrentAtion_Of_ActionListDO()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curService = new ActionList();
                Assert.IsNotNull(uow.ActionListRepository.GetByKey(1));
                curService.Process(uow.ActionListRepository.GetByKey(1));
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

        void InitializeActionList()
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
                actionList.ActionListType = 1;
                actionList.CurrentAction = null;
                uow.ActionListRepository.Add(actionList);
                uow.SaveChanges();
            }
        }

        #endregion
    }

}
