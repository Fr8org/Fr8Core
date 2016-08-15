using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Data.Entities;
using Data.States;
using Hub.Interfaces;
using NUnit.Framework;
using Fr8.Testing.Unit;
using Fr8.Testing.Unit.Fixtures;
using StructureMap;
using Data.Interfaces;
using Fr8.Infrastructure.Data.States;

namespace HubTests.Services
{
    [TestFixture]
    [Category("PlanNode")]
    public class PlanNodeTests : BaseTest
    {
        private IPlanNode _planNode;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            FixtureData.AddTestActivityTemplate();
            _planNode = ObjectFactory.GetInstance<IPlanNode>();
        }

        [Test]
        public void GetAvailableData_ShouldReturnFields()
        {
            var plan = new PlanDO();
            plan.Name = "sdfasdfasdf";
            plan.PlanState = PlanState.Executing;
            var testActionTree = FixtureData.TestActivity2Tree();

            plan.ChildNodes.Add(testActionTree);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.PlanRepository.Add(plan);
                uow.SaveChanges();
            }

            IPlanNode planNodeService = ObjectFactory.GetInstance<IPlanNode>();
            var fieldsCrate = planNodeService.GetIncomingData(testActionTree.ChildNodes.Last().Id, CrateDirection.Upstream, AvailabilityType.NotSet);
            Assert.NotNull(fieldsCrate);
            Assert.NotNull(fieldsCrate.AvailableCrates.SelectMany(x=> x.Fields).ToList());
            Assert.AreEqual(33, fieldsCrate.AvailableCrates.SelectMany(x => x.Fields).ToList().Count);
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
                    PlanState = PlanState.Executing,
                    ChildNodes = { root }
                });

                uow.SaveChanges();

                PlanNodeDO curActivity = root;
                List<PlanNodeDO> seqToTest = new List<PlanNodeDO>();
                List<PlanNodeDO> refSeq = new List<PlanNodeDO>();

                TraverseActivities(root, refSeq);

                do
                {
                    seqToTest.Add(curActivity);
                    curActivity = _planNode.GetNextActivity(curActivity, null);
                } while (curActivity != null);

                Assert.AreEqual(refSeq.Count, seqToTest.Count);

                for (int i = 0; i < refSeq.Count; i++)
                {
                    Assert.AreEqual(refSeq[i], seqToTest[i]);
                }
            }
        }

        private void TraverseActivities(PlanNodeDO root, List<PlanNodeDO> seq)
        {
            seq.Add(root);

            foreach (var activityDo in root.ChildNodes.OrderBy(x => x.Ordering))
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
                    PlanState = PlanState.Executing,
                    ChildNodes = { FixtureData.TestActivityTree() }
                });

                uow.SaveChanges();


                PlanNodeDO curActivity = uow.PlanRepository.GetById<PlanNodeDO>(FixtureData.TestActivity57().Id);

                List<PlanNodeDO> upstreamActivities = _planNode.GetUpstreamActivities(uow, curActivity).Where(x => !(x is PlanDO)).ToList();
                foreach (var activity in upstreamActivities)
                {
                    Debug.WriteLine(FixtureData.GetTestIdByGuid(activity.Id));
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
                    PlanState = PlanState.Executing,
                    ChildNodes = { FixtureData.TestActivityTree() }
                });

                uow.SaveChanges();


                PlanNodeDO curActivity = FixtureData.TestActivity57();
                List<PlanNodeDO> downstreamActivities = _planNode.GetDownstreamActivities(uow, uow.PlanRepository.GetById<PlanNodeDO>(curActivity.Id));
                foreach (var activity in downstreamActivities)
                {
                    Debug.WriteLine(FixtureData.GetTestIdByGuid(activity.Id));
                }

                Assert.AreEqual(9, downstreamActivities.Count());

            }
        }
    }
}
