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
        private Mock<IAction> actionMock;
        [SetUp]
        public override void SetUp()
        {
 	        base.SetUp();
           _actionList = ObjectFactory.GetInstance<IActionList>();
        }
        

        [Test]
        [ExpectedException(ExpectedMessage = "Action List ID: 2 status is not unstarted.")]
        public void Process_ActionListNotUnstarted_ThrowException()
        {
            ActionListDO actionListDo = FixtureData.TestActionList3();
            ActionList _actionList = ObjectFactory.GetInstance<ActionList>();
            
            _actionList.Process(actionListDo);
        }

        [Test]
        public void Process_ActionListUnstarted_SetToComplete()
        {
            ActionListDO actionListDo = FixtureData.TestActionList4();
            ActionList _actionList = ObjectFactory.GetInstance<ActionList>();

            _actionList.Process(actionListDo);

            Assert.AreEqual(ActionListState.Completed, actionListDo.ActionListState);
        }

        [Test]
        public void Process_CurrentActionInLastList_SetToComplete()
        {
            ActionListDO actionListDO = FixtureData.TestActionList4();

            actionMock = new Mock<IAction>();
            actionMock.Setup(s => s.Process((ActionDO)It.IsAny<object>())).Callback<ActionDO>(p => { p.ActionState = ActionState.Completed; });
            ObjectFactory.Configure(cfg => cfg.For<IAction>().Use(actionMock.Object));
            
            _actionList.Process(actionListDO);

            Assert.AreEqual(ActionState.Completed, actionListDO.CurrentAction.ActionState);
        }
    }
}
