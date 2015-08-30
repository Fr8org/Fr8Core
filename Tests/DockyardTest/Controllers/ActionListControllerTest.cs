using System;
using System.Linq;
using System.Web.Http.Results;
using NUnit.Framework;
using StructureMap;
using Core.Services;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Web.Controllers;
using Web.ViewModels;
using DockyardTest.Controllers.Api;

namespace DockyardTest.Controllers
{
    [TestFixture]
    public class ActionListControllerTest : ApiControllerTestBase
    {
        private ProcessNodeTemplateDO _curProcessNodeTemplate;
        private ActionListDO _curActionList;

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
                Assert.AreEqual(uow.ActionListRepository.GetByKey(1).CurrentActivity.Ordering, 1);
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

        [Test]
        [Category("ActionListController.GetByProcessNodeTemplateId")]
        public void ActionListController_GetByProcessNodeTemplateId()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var controller = CreateController<ActionListController>();

                var actionResult = controller.GetByProcessNodeTemplateId(
                    _curProcessNodeTemplate.Id, ActionListType.Immediate);

                var okResult = actionResult as OkNegotiatedContentResult<ActionListDTO>;

                Assert.IsNotNull(okResult);
                Assert.IsNotNull(okResult.Content);
                Assert.AreEqual(okResult.Content.Id, _curActionList.Id);
            }
        }

        #region Private methods
        /// <summary>
        /// Creates a new Action with the given actiond ID
        /// </summary>
        private ActionDO CreateActionDO()
        {
            var actionTemplate = FixtureData.ActionTemplate();

            return new ActionDO
            {
                Id = 10,
                UserLabel = "AzureSqlAction",
                Name = "WriteToAzureSql",
                ActionListId = 1,
                ConfigurationSettings = "JSON Config Settings",
                FieldMappingSettings = "JSON Field Mapping Settings",
                ParentPluginRegistration = "AzureSql",
                Ordering = 1,
                ActionState = ActionState.Unstarted,
                ActionTemplateId = actionTemplate.Id,
                ActionTemplate = actionTemplate

            };
        }

        private void InitializeActionList()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Add a template
                _curProcessNodeTemplate = FixtureData.TestProcessNodeTemplateDO1();
                uow.ProcessNodeTemplateRepository.Add(_curProcessNodeTemplate);
                uow.SaveChanges();

                _curActionList = FixtureData.TestActionList();
                _curActionList.ActionListType = ActionListType.Immediate;
                _curActionList.CurrentActivity = null;
                _curActionList.ProcessNodeTemplateID = _curProcessNodeTemplate.Id;

                uow.ActionListRepository.Add(_curActionList);
                uow.SaveChanges();
            }
        }

        #endregion
    }

}
