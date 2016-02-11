using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Constants;
using Moq;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Hub.Interfaces;
using Hub.Services;
using Hub.StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Services
{
    [TestFixture]
    [Category("Activity")]
    public class ActivityTests : BaseTest
    {
        private IRouteNode _activity;
        //private Mock<IAction> _actionMock;
        private SubrouteDO _curSubroute;
        [SetUp]
        public override void SetUp()
        {
 	        base.SetUp();
            _activity = ObjectFactory.GetInstance<IRouteNode>();

        }

        [Test]
        public void Activity_CheckGetNextActivities()
        {
            // If allis working right than iterative execution of GetNextActivity would return 
            // the sequence of activities equals to the sequence of activity tree depth-first traversal with 
            // children visited according to their ordering

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var root = FixtureData.TestActivityTree();

                uow.PlanRepository.Add(new PlanDO()
                {
                    Name = "name",
                    RouteState = RouteState.Active,
                    ChildNodes = { root }
                });
                
                uow.SaveChanges();

                RouteNodeDO curActivity = root;
                List<RouteNodeDO> seqToTest = new List<RouteNodeDO>();
                List<RouteNodeDO> refSeq = new List<RouteNodeDO>();

                TraverseActivities(root, refSeq);

                do
                {
                    seqToTest.Add(curActivity);
                    curActivity = _activity.GetNextActivity(curActivity, null);
                } while (curActivity != null);

                Assert.AreEqual(refSeq.Count, seqToTest.Count);

                for (int i = 0; i < refSeq.Count; i ++)
                {
                    Assert.AreEqual(refSeq[i], seqToTest[i]);
                }
            }
        }

        private void TraverseActivities(RouteNodeDO root, List<RouteNodeDO> seq)
        {
            seq.Add(root);

            foreach (var activityDo in root.ChildNodes.OrderBy(x=>x.Ordering))
            {
                TraverseActivities(activityDo, seq);
            }
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

                uow.PlanRepository.Add(new PlanDO()
                {
                    Name = "name",
                    RouteState = RouteState.Active,
                    ChildNodes = { FixtureData.TestActivityTree() }
                });
                
                uow.SaveChanges();


                RouteNodeDO curActivity = uow.PlanRepository.GetById<RouteNodeDO>(FixtureData.TestActivity57().Id);

                List<RouteNodeDO> upstreamActivities = _activity.GetUpstreamActivities(uow, curActivity).Where(x => !(x is PlanDO)).ToList();
                foreach (var activity in upstreamActivities)
                {
                    Debug.WriteLine (FixtureData.GetTestIdByGuid(activity.Id));
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
                uow.PlanRepository.Add(new PlanDO()
                {
                    Name = "name",
                    RouteState = RouteState.Active,
                    ChildNodes = { FixtureData.TestActivityTree() }
                });
                
                //uow.RouteNodeRepository.Add(FixtureData.TestActivityTree());
                uow.SaveChanges();


                RouteNodeDO curActivity = FixtureData.TestActivity57();
                List<RouteNodeDO> downstreamActivities = _activity.GetDownstreamActivities(uow, uow.PlanRepository.GetById<RouteNodeDO>(curActivity.Id));
                foreach (var activity in downstreamActivities)
                {
                    Debug.WriteLine(FixtureData.GetTestIdByGuid(activity.Id));
                }

                Assert.AreEqual(9, downstreamActivities.Count());

            }
        }
        

//        [Test]
//        [ExpectedException(typeof(ArgumentNullException))]
//        public void GetNextActivities_ActivityDOIsNull_ThorwArgumentNullException()
//        {
//            _activity = ObjectFactory.GetInstance<IActivity>();
//            _activity.GetNextActivities(null);
//        }
//
//        [Test]
//        public void GetNextActivities_ActivityDOActivityNullOrNotNull()
//        {          
//            var activity = FixtureData.TestActivityNotExists();
//          var result=_activity.GetNextActivities(activity).ToList();
//          if (result.Count > 0)
//          {
//              Assert.Greater(result[0].Ordering, activity.Ordering);
//          }
//          else
//          { 
//              Assert.IsEmpty(result);
//          }
//           
//        }

        [Test]
        public void Process_curActivityDOIsNull()
        {
            _activity = ObjectFactory.GetInstance<IRouteNode>();
            var containerDO = FixtureData.TestContainer1();
            Task result = _activity.Process(It.IsAny<Guid>(), It.IsAny<ActionState>(), containerDO);
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

                var activity = FixtureData.TestActivity1();
                //uow.ActivityRepository.Add(activity);
                uow.SaveChanges();

                ActivityDO obj = FixtureData.TestActivityProcess();
               // uow.RouteNodeRepository.Add(obj);
                uow.SaveChanges();

                ContainerDO containerDO = FixtureData.TestContainer1();
                _activity = ObjectFactory.GetInstance<IRouteNode>();
                _activity.Process(FixtureData.GetTestGuidById(1), It.IsAny<ActionState>(), containerDO);
            }
        }

    // Commented out by Vladimir. There is no ActionLists now. What this test is going to check?
//        [Test]
//        public void Process_curActivityDOIsActionListDO()
//        {
//            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
//            {
//
//                ActivityTemplateDO templateObj = FixtureData.TestActivityTemplate1();
//                uow.ActivityTemplateRepository.Add(templateObj);
//                uow.SaveChanges();
//
//                var action = FixtureData.TestAction1();
//                uow.ActionRepository.Add(action);
//                uow.SaveChanges();
//
//                ActionDO obj = FixtureData.TestActionProcess();
//                uow.ActivityRepository.Add(obj);
//                uow.SaveChanges();
//
//                ActionListDO listObj = FixtureData.TestActionListProcess();
//                listObj.Activities.Add(obj);
//                uow.ActivityRepository.Add(listObj);
//                uow.SaveChanges();
//
//                ProcessDO processDo = FixtureData.TestProcess1();               
//
//                _activity = ObjectFactory.GetInstance<IActivity>();
//                _activity.Process(52, processDo);
//            }
//        }

    }
}
