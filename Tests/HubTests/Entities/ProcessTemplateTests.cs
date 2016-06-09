using System.Linq;
using Data.Entities;
using NUnit.Framework;
using StructureMap;
using Data.Interfaces;
using Fr8.Infrastructure.Data.Manifests;
using Hub.Interfaces;
using Fr8.Testing.Unit;
using Fr8.Testing.Unit.Fixtures;

namespace HubTests.Entities
{
    [TestFixture]
    [Category("Plan")]
    public class PlanTests : BaseTest
    {
        [Test]
        public void Plan_ShouldBeAssignedStartingSubPlan()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = FixtureData.TestPlan2();
                var subPlan = FixtureData.TestSubPlanDO2();

                plan.ChildNodes.Add(subPlan);
                uow.PlanRepository.Add(plan);
                plan.StartingSubplan = subPlan;

                uow.SaveChanges();

                var result = uow.PlanRepository.GetById<PlanDO>(plan.Id);//.SingleOrDefault(pt => pt.StartingSubPlanId == subPlan.Id);

                Assert.AreEqual(subPlan.Id, result.StartingSubplan.Id);
                Assert.AreEqual(subPlan.Name, result.StartingSubplan.Name);
            }
        }

        [Test]
        public void GetStandardEventSubscribers_ReturnsPlans()
        {
            FixtureData.TestPlanWithSubscribeEvent();
            IPlan curPlan = ObjectFactory.GetInstance<IPlan>();
            EventReportCM curEventReport = FixtureData.StandardEventReportFormat();

            var result = curPlan.GetMatchingPlans("testuser1", curEventReport);

            Assert.IsNotNull(result);
            Assert.Greater(result.Count, 0);
            Assert.Greater(result.Where(name => name.Name.Contains("StandardEventTesting")).Count(), 0);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(System.ArgumentNullException))]
        public void GetStandardEventSubscribers_UserIDEmpty_ThrowsException()
        {
            IPlan curPlan = ObjectFactory.GetInstance<IPlan>();

            curPlan.GetMatchingPlans("", new EventReportCM());
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(System.ArgumentNullException))]
        public void GetStandardEventSubscribers_EventReportMSNULL_ThrowsException()
        {
            IPlan curPlan = ObjectFactory.GetInstance<IPlan>();

            curPlan.GetMatchingPlans("UserTest", null);
        }


    }
}