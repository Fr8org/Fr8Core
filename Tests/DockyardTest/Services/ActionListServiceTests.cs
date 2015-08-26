using Core.Interfaces;
using Core.Services;
using Data.Entities;
using Data.States;
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
    }
}
