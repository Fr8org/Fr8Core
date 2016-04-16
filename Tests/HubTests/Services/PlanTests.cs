using System.Linq;
using Data.Constants;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using HubTests.Services.Container;
using Hub.Interfaces;
using NUnit.Framework;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using InternalInterface = Hub.Interfaces;

namespace HubTests.Services
{
    [TestFixture]
    [Category("Plan")]
    public class PlanTests : ContainerExecutionTestBase
    {

        // MockDB has boken logic when working with collections of objects of derived types
        // We add object to PlanRepository but Delete logic recusively traverse Activity repository.
        [Ignore("MockDB behavior is incorrect")]
        [Test]
        public void PlanService_CanCreate()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curPlanDO = FixtureData.TestPlan_CanCreate();
                var curUserAccount = FixtureData.TestDockyardAccount1();
                curPlanDO.Fr8Account = curUserAccount;

                Plan.CreateOrUpdate(uow, curPlanDO, false);

                uow.SaveChanges();

                var result = uow.PlanRepository.GetById<PlanDO>(curPlanDO.Id);
                Assert.NotNull(result);
                Assert.AreNotEqual(result.Id, 0);
                Assert.NotNull(result.StartingSubPlan);
                Assert.AreEqual(result.SubPlans.Count(), 1);
                Assert.AreEqual(result.StartingSubPlan.ChildNodes.Count, 2);
            }
        }

        [Test]
        public void PlanService_CanDelete()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curPlanDO = FixtureData.TestPlanWithStartingSubPlans_ID0();
                uow.PlanRepository.Add(curPlanDO);
                uow.SaveChanges();

                Assert.AreNotEqual(curPlanDO.Id, 0);

                var currPlanDOId = curPlanDO.Id;
                Plan.Delete(uow, curPlanDO.Id);
                var result = uow.PlanRepository.GetById<PlanDO>(currPlanDOId);

                Assert.NotNull(result);
            }
        }
        
        [Test]
        [Ignore("ActivityTemplates are not being added to ActivityTemplate respository. Should be fixed if test is needed")]
        public void Activate_NoMatchingParentActivityId_ReturnsNoActivity()
        {
            var curPlanDO = FixtureData.TestPlanNoMatchingParentActivity();

            var result = Plan.Activate(curPlanDO.Id, true).Result;

            Assert.AreEqual(result.Status, "no activity");
        }
    }
}