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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Services
{
    [TestFixture]
    [Category("Activity")]
    public class ActivityTests : BaseTest
    {
        private IActivity _activity;
        private Mock<IAction> _actionMock;
        private ProcessNodeTemplateDO _curProcessNodeTemplate;
        private ActionListDO _curActionList;
        [SetUp]
        public override void SetUp()
        {
 	        base.SetUp();
            _activity = ObjectFactory.GetInstance<IActivity>();

        }


        [Test]
        
        public void Activity_CanGetUpstreamActivities()
        {
            //load tree
            //set cur activity to id 57
            //verify that the count of upstream activities is 10
            //print out the list of activity ids
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ActivityRepository.Add(FixtureData.TestActivityTree());
                uow.SaveChanges();


                ActivityDO curActivity = FixtureData.TestActivity57();

                List<ActivityDO> upstreamActivities = _activity.GetUpstreamActivities(curActivity);
                foreach (var activity in upstreamActivities)
                {
                    Debug.WriteLine(activity.Id);
                }

                Assert.AreEqual(10, upstreamActivities.Count());

            }
        }

        [Test]
        public void Activity_CanGetDownstreamActivities()
        {
            //load tree
            //set cur activity to id 57
            //verify that the count of upstream activities is 10
            //print out the list of activity ids
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ActivityRepository.Add(FixtureData.TestActivityTree());
                uow.SaveChanges();


                ActivityDO curActivity = FixtureData.TestActivity57();

                List<ActivityDO> downstreamActivities = _activity.GetDownstreamActivities(curActivity);
                foreach (var activity in downstreamActivities)
                {
                    Debug.WriteLine(activity.Id);
                }

                Assert.AreEqual(9, downstreamActivities.Count());

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

        [Test]
        public void Process_curActivityDOIsNull_ThorwArgumentNullException()
        {
            _activity = ObjectFactory.GetInstance<IActivity>();
            var processDo = FixtureData.TestProcess1();
            Task result = _activity.Process(It.IsAny<int>(), processDo);
            Assert.AreEqual(result.Exception.InnerException.Message, "Cannot find Activity with the supplied curActivityId");
        }

        [Test]
        public void Process_curActivityDOIsActionDO()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {

                ActivityTemplateDO templateObj = FixtureData.TestActivityTemplate1();
                uow.ActivityTemplateRepository.Add(templateObj);
                uow.SaveChanges();

                var action = FixtureData.TestAction1();
                uow.ActionRepository.Add(action);
                uow.SaveChanges();

                ActionDO obj = FixtureData.TestActionForProcess();
                uow.ActivityRepository.Add(obj);
                uow.SaveChanges();

                ProcessDO processDo = FixtureData.TestProcess1();
                _activity = ObjectFactory.GetInstance<IActivity>();
                _activity.Process(1, processDo);
            }
        }


        [Test]
        public void Process_curActivityDOIsActionListDO()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {

                ActivityTemplateDO templateObj = FixtureData.TestActivityTemplate1();
                uow.ActivityTemplateRepository.Add(templateObj);
                uow.SaveChanges();

                var action = FixtureData.TestAction1();
                uow.ActionRepository.Add(action);
                uow.SaveChanges();

                ActionDO obj = FixtureData.TestActionForProcess();
                uow.ActivityRepository.Add(obj);
                uow.SaveChanges();

                ActionListDO listObj = new ActionListDO() { Id = 52, Ordering = 3, ActionListType = ActionListType.Immediate, Name = "al_52" };
                listObj.Activities.Add(obj);
                uow.ActivityRepository.Add(listObj);
                uow.SaveChanges();

                ProcessDO processDo = FixtureData.TestProcess1();

                //var _activity = new Mock<IActivity>();
                //_activity
                //    .Setup(c => c.Process(1, processDo))
                //    .Returns(Task.Delay(100))
                //    .Verifiable();

                _activity = ObjectFactory.GetInstance<IActivity>();
                _activity.Process(52, processDo);
            }
        }

    }
}
