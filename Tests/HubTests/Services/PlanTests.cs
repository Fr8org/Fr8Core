using System;
using System.Linq;
using System.Threading.Tasks;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using HubTests.Services.Container;
using NUnit.Framework;
using Fr8.Testing.Unit.Fixtures;

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

                Plan.CreateOrUpdate(uow, curPlanDO);

                uow.SaveChanges();

                var result = uow.PlanRepository.GetById<PlanDO>(curPlanDO.Id);
                Assert.NotNull(result);
                Assert.AreNotEqual(result.Id, 0);
                Assert.NotNull(result.StartingSubplan);
                Assert.AreEqual(result.SubPlans.Count(), 1);
                Assert.AreEqual(result.StartingSubplan.ChildNodes.Count, 2);
            }
        }

        [Test]
        public async Task PlanService_CanDelete()
        {
            Guid planId;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curPlanDO = FixtureData.TestPlanWithStartingSubPlans_ID0();

                uow.PlanRepository.Add(curPlanDO);
                uow.SaveChanges();

                Assert.AreNotEqual(curPlanDO.Id, 0);

                planId = curPlanDO.Id;
            }

            await Plan.Delete(planId);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var result = uow.PlanRepository.GetById<PlanDO>(planId);

                Assert.NotNull(result);
            }
        }
    }
}