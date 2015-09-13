using Core.Interfaces;
using Core.Services;
using Core.StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Moq;
using NUnit.Framework;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Services
{
    [TestFixture]
    [Category("ActionListService")]
    public class ActionListServiceTests : BaseTest
    {
        private IActionList _actionList;
        private Mock<IAction> _actionMock;
        private ProcessNodeTemplateDO _curProcessNodeTemplate;
        private ActionListDO _curActionList;
        [SetUp]
        public override void SetUp()
        {
 	        base.SetUp();
            InitializeActionList();
        }
        

        [Test,Ignore]
        [ExpectedException(ExpectedMessage = "Action List ID: 2 status is not unstarted.")]
        public void Process_ActionListNotUnstarted_ThrowException()
        {
            ActionListDO actionListDo = FixtureData.TestActionList3();
            ProcessDO processDO = FixtureData.TestProcess1();
            _actionList = ObjectFactory.GetInstance<IActionList>();

            _actionList.Process(actionListDo, processDO);
        }

        [Test,Ignore]
        public void Process_CurrentActionInLastList_SetToComplete()
        {
            ActionListDO actionListDO = FixtureData.TestActionList7();
            ProcessDO processDO = FixtureData.TestProcess1();
            _actionMock = new Mock<IAction>();
            _actionMock.Setup(s => s.Process((ActionDO)It.IsAny<object>(), It.IsAny<ProcessDO>())).Callback<ActionDO>(p => { p.ActionState = ActionState.Completed; });
            ObjectFactory.Configure(cfg => cfg.For<IAction>().Use(_actionMock.Object));
            _actionList = ObjectFactory.GetInstance<IActionList>();
            
            _actionList.Process(actionListDO, processDO);

            ActionDO actionDO = new ActionDO();
            if (actionListDO.CurrentActivity is ActionDO)
                actionDO = (ActionDO)actionListDO.CurrentActivity;
            Assert.AreEqual(ActionState.Completed, actionDO.ActionState);
            Assert.AreEqual(ActionListState.Completed, actionListDO.ActionListState);
        }


        [Test,Ignore]
        public void Process_CurrentActionInLastList_EqualToCurrentAction()
        {
            ActionListDO actionListDO = FixtureData.TestActionList7();
            ProcessDO processDO = FixtureData.TestProcess1();
            ActionDO lastActionDO = actionListDO
                .Activities
                .OfType<ActionDO>()
                .OrderByDescending(o => o.Ordering).FirstOrDefault();
            _actionMock = new Mock<IAction>();
            _actionMock.Setup(s => s.Process((ActionDO)It.IsAny<object>(), It.IsAny<ProcessDO>())).Callback<ActionDO>(p => { p.ActionState = ActionState.Completed; });
            ObjectFactory.Configure(cfg => cfg.For<IAction>().Use(_actionMock.Object));
            _actionList = ObjectFactory.GetInstance<IActionList>();

            _actionList.Process(actionListDO, processDO);

            ActionDO actionDO = new ActionDO();
            if (actionListDO.CurrentActivity is ActionDO)
                actionDO = (ActionDO)actionListDO.CurrentActivity;
            Assert.AreEqual(actionDO.Id, lastActionDO.Id);
        }

        [Test]
        [Ignore("Requires update after v2 changes.")]
        [ExpectedException(ExpectedMessage = "Action List ID: 2. Action status returned: 4")]
        public void Process_ActionListCurrentActionNotCompletedAndInProcess_ThrowException()
        {
            ActionListDO actionListDO = FixtureData.TestActionList7();
            ProcessDO processDO = FixtureData.TestProcess1();
            actionListDO.ActionListState = ActionListState.Unstarted;
            var actionDO = (ActionDO)actionListDO.CurrentActivity;
            actionDO.ActionState = ActionState.Completed;
            actionListDO.CurrentActivity = actionDO;
            _actionMock = new Mock<IAction>();
            _actionMock.Setup(s => s.Process((ActionDO)It.IsAny<object>(), It.IsAny<ProcessDO>())).Callback<ActionDO>(p => { p.ActionState = ActionState.Error; });
            ObjectFactory.Configure(cfg => cfg.For<IAction>().Use(_actionMock.Object));
            _actionList = ObjectFactory.GetInstance<IActionList>();

            _actionList.Process(actionListDO, processDO);
        }

        [Test]
        [ExpectedException(ExpectedMessage = "Action List ID: 2. Action status returned: 4")]
        public void SetCurrentActivityPointer_SetCurrentActivityStateNoCompletedAndInProcess_ThrowException()
        {
            ActionListDO actionListDO = FixtureData.TestActionList7();
            var actionDO = (ActionDO)actionListDO.CurrentActivity;
            actionDO.ActionState = ActionState.Error;
            actionListDO.CurrentActivity = actionDO;

            _actionList = ObjectFactory.GetInstance<IActionList>();

            _actionList.UpdateActionListState(actionListDO);
        }

        [Test]
        public void SetCurrentActivityPointer_SetCurrentActivityStateCompletedAndInProcess_MoveToNextAction()
        {
            ActionListDO actionListDO = FixtureData.TestActionList7();
            //CurrentAcvitityID = 6 and the next ActionID is 7
            ((ActionDO)actionListDO.CurrentActivity).ActionState = ActionState.Completed;
            _actionList = ObjectFactory.GetInstance<IActionList>();

            _actionList.UpdateActionListState(actionListDO);

            ActionDO actionDO = new ActionDO();
            if (actionListDO.CurrentActivity is ActionDO)
                actionDO = (ActionDO)actionListDO.CurrentActivity;
            Assert.AreEqual(actionDO.Id, 7);
        }

        [Test,Ignore]
        public void ProcessNextActivity_CheckLastActionOrder_EqualToCurrentActivity()
        {
            ActionListDO actionListDO = FixtureData.TestActionList7();
            ProcessDO processDO = FixtureData.TestProcess1(); 
            ActionDO lastActionDO = actionListDO.Activities
                .OfType<ActionDO>()
                .OrderByDescending(o => o.Ordering).FirstOrDefault();
            _actionMock = new Mock<IAction>();
            _actionMock.Setup(s => s.Process((ActionDO)It.IsAny<object>(), It.IsAny<ProcessDO>())).Callback<ActionDO>(p => { p.ActionState = ActionState.Completed; });
            ObjectFactory.Configure(cfg => cfg.For<IAction>().Use(_actionMock.Object));
            _actionList = ObjectFactory.GetInstance<IActionList>();

            _actionList.ProcessAction(actionListDO, processDO);

            ActionDO actionDO = new ActionDO();
            if (actionListDO.CurrentActivity is ActionDO)
                actionDO = (ActionDO)actionListDO.CurrentActivity;
            Assert.AreEqual(actionDO.Id, lastActionDO.Id);
        }

        [Test,Ignore]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public void ProcessNextActivity_NextActionListDONull_ThrowException()
        {
            ActionListDO actionListDO = FixtureData.TestActionList7();
            ActionListDO actionListDONext = FixtureData.TestActionList7();
            ProcessDO processDO = FixtureData.TestProcess1();
            actionListDONext.CurrentActivity = null;
            actionListDO.CurrentActivity = actionListDONext;

            _actionMock = new Mock<IAction>();
            _actionMock.Setup(s => s.Process((ActionDO)It.IsAny<object>(), It.IsAny<ProcessDO>())).Callback<ActionDO>(p => { p.ActionState = ActionState.Completed; });
            ObjectFactory.Configure(cfg => cfg.For<IAction>().Use(_actionMock.Object));
            _actionList = ObjectFactory.GetInstance<IActionList>();

            _actionList.ProcessAction(actionListDO, processDO);
        }

        [Test]
        public void ActionListController_CanAddActionDOInLastPosition()
        {
            _actionList = ObjectFactory.GetInstance<IActionList>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                
                ActionDO actionDO = FixtureData.TestAction22();
                _actionList.AddAction(actionDO, "last");
                Assert.IsNotNull(uow.ActionRepository.GetByKey(10));
                Assert.AreEqual(uow.ActionRepository.GetByKey(10).Ordering, 3);
            }
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(NotSupportedException))]
        public void ActionListController_CanAddActionDOInPositionOtherThanLast()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _actionList = ObjectFactory.GetInstance<IActionList>();
                ActionDO actionDO = FixtureData.TestAction22();
                _actionList.AddAction(actionDO, "first");
            }
        }

        [Test]
        public void ActionListController_Set_LowestPostitoned_ActionDO_To_CurrentAtion_Of_ActionListDO()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _actionList = ObjectFactory.GetInstance<IActionList>();
                ActionDO actionDO = FixtureData.TestAction22();
                _actionList.AddAction(actionDO, "last");
                Assert.IsNotNull(uow.ActionListRepository.GetByKey(1));
                Assert.AreEqual(uow.ActionListRepository.GetByKey(1).CurrentActivity.Ordering, 1);
            }
        }

        [Test]
        public void ActionListController_CanGetByKeyAndNotNull()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _actionList = ObjectFactory.GetInstance<IActionList>();
                Assert.IsNotNull(uow.ActionListRepository.GetByKey(1));
            }
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

    }
}
