using Core.Interfaces;
using Core.Services;
using Core.StructureMap;
using Data.Entities;
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
        [SetUp]
        public override void SetUp()
        {
 	        base.SetUp();
        }
        

        [Test]
        [ExpectedException(ExpectedMessage = "Action List ID: 2 status is not unstarted.")]
        public void Process_ActionListNotUnstarted_ThrowException()
        {
            ActionListDO actionListDo = FixtureData.TestActionList3();
            _actionList = ObjectFactory.GetInstance<IActionList>();

            _actionList.Process(actionListDo);
        }

        [Test]
        public void Process_CurrentActionInLastList_SetToComplete()
        {
            ActionListDO actionListDO = FixtureData.TestActionList7();
            _actionMock = new Mock<IAction>();
            _actionMock.Setup(s => s.Process((ActionDO)It.IsAny<object>())).Callback<ActionDO>(p => { p.ActionState = ActionState.Completed; });
            ObjectFactory.Configure(cfg => cfg.For<IAction>().Use(_actionMock.Object));
            _actionList = ObjectFactory.GetInstance<IActionList>();
            
            _actionList.Process(actionListDO);

            Assert.AreEqual(ActionState.Completed, actionListDO.CurrentAction.ActionState);
            Assert.AreEqual(ActionListState.Completed, actionListDO.ActionListState);
        }


        [Test]
        public void Process_CurrentActionInLastList_EqualToCurrentAction()
        {
            ActionListDO actionListDO = FixtureData.TestActionList7();
            ActionDO lastActionDO = actionListDO.Actions.OrderByDescending(o => o.Ordering).FirstOrDefault();
            _actionMock = new Mock<IAction>();
            _actionMock.Setup(s => s.Process((ActionDO)It.IsAny<object>())).Callback<ActionDO>(p => { p.ActionState = ActionState.Completed; });
            ObjectFactory.Configure(cfg => cfg.For<IAction>().Use(_actionMock.Object));
            _actionList = ObjectFactory.GetInstance<IActionList>();

            _actionList.Process(actionListDO);


            Assert.AreEqual(actionListDO.CurrentAction.Id, lastActionDO.Id);
        }

        [Test]
        [ExpectedException(ExpectedMessage = "Action List ID: 2. Action status returned: 4")]
        public void Process_ActionListCurrentActionNotCompletedAndInProcess_ThrowException()
        {
            ActionListDO actionListDO = FixtureData.TestActionList7();
            actionListDO.ActionListState = ActionListState.Unstarted;
            actionListDO.CurrentAction.ActionState = ActionState.Completed;
            _actionMock = new Mock<IAction>();
            _actionMock.Setup(s => s.Process((ActionDO)It.IsAny<object>())).Callback<ActionDO>(p => { p.ActionState = ActionState.Error; });
            ObjectFactory.Configure(cfg => cfg.For<IAction>().Use(_actionMock.Object));
            _actionList = ObjectFactory.GetInstance<IActionList>();

            _actionList.Process(actionListDO);
        }
    }
}
