using System;
using System.Linq;
using Data.Entities;
using Data.Repositories.Cache;
using Data.Repositories.Plan;
using Hub.StructureMap;
using NUnit.Framework;
using Fr8.Testing.Unit;

namespace HubTests.Repositories.Plan
{
    internal class ExpirationStrategyMock : IPlanCacheExpirationStrategy
    {
        private class ExpirationToken : IExpirationToken
        {
            public bool IsExpired()
            {
                return true;
            }
        }

        private Action _callback;

        public void SetExpirationCallback(Action callback)
        {
            _callback = callback;
        }

        public void InvokeExpirationCallback()
        {
            var cb = _callback;
            
            if (cb != null)
            {
                cb();
            }
        }

        public IExpirationToken NewExpirationToken()
        {
            return new ExpirationToken();
        }
    }

    [TestFixture]
    [Category("PlanRepositoryTests")]
    public class PlanCacheTests : BaseTest
    {
        [SetUp]
        public void Setup()
        {
            base.SetUp();
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
        }

        private PlanNodeDO LoadPlan(Guid arg, string postfix = "")
        {
            return new PlanDO
            {
                Id = arg,
                Name = "Plan",
                ChildNodes =
                {
                    new ActivityDO()
                    {
                        Id = new Guid(2, (short) 0, (short) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0),
                        Label = "Base1"+postfix,
                    },
                    new ActivityDO()
                    {
                        Id = new Guid(3, (short) 0, (short) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0),
                        Label = "Base2"+postfix,
                    }
                }
            };
        }
        

        [Test]
        public void CanLoadOnCacheMiss()
        {
            var expiration = new ExpirationStrategyMock();
            var planId = new Guid(1, (short) 0, (short) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0);
            int calledTimes = 0;

            Func<Guid, PlanNodeDO> cacheMiss = x =>
            {
                calledTimes++;
                Assert.AreEqual(planId, x);
                return LoadPlan(x);
            };

            var cache = new PlanCache(expiration);

            cache.Get(planId, cacheMiss);

            Assert.AreEqual(1, calledTimes);
        }

        [Test]
        public void CanExpireItems()
        {
            var expiration = new ExpirationStrategyMock();
            var planId = new Guid(1, (short)0, (short)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0);
            int calledTimes = 0;

            Func<Guid, PlanNodeDO> cacheMiss = x =>
            {
                calledTimes++;
                Assert.AreEqual(planId, x);
                return LoadPlan(x);
            };

            var cache = new PlanCache(expiration);


            cache.Get(planId, cacheMiss);
            expiration.InvokeExpirationCallback();
            cache.Get(planId, cacheMiss);

            Assert.AreEqual(2, calledTimes);
        }

        [Test]
        public void CanLoadFromCache()
        {
            var expiration = new ExpirationStrategyMock();
            var planId = new Guid(1, (short)0, (short)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0);
            int calledTimes = 0;

            var plan = LoadPlan(planId);

            Func<Guid, PlanNodeDO> cacheMiss = x =>
            {
                calledTimes++;
                return plan;
            };

            var cache = new PlanCache(expiration);

            var plan1 = cache.Get(planId, cacheMiss);

            Assert.IsTrue(AreEquals(plan1, plan));
            
            foreach (var id in PlanTreeHelper.Linearize(plan).Select(x=>x.Id))
            {
                Assert.IsTrue(AreEquals(plan, cache.Get(id, cacheMiss)));
            }

            Assert.AreEqual(1, calledTimes);
        }

        [Test]
        public void CanLoadFromCacheUsingChildActivitiesId()
        {
            var expiration = new ExpirationStrategyMock();
            var planId = new Guid(1, (short)0, (short)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0);
            int calledTimes = 0;

            var plan = LoadPlan(planId);

            Func<Guid, PlanNodeDO> cacheMiss = x =>
            {
                calledTimes++;
                return plan;
            };

            var cache = new PlanCache(expiration);

            var plan1 = cache.Get(new Guid(2, (short)0, (short)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0), cacheMiss);

            Assert.IsTrue(AreEquals(plan1, plan));

            foreach (var id in PlanTreeHelper.Linearize(plan).Select(x => x.Id))
            {
                Assert.IsTrue(AreEquals(plan, cache.Get(id, cacheMiss)));
            }

            Assert.AreEqual(1, calledTimes);
        }


        [Test]
        public void CanUpdateCache()
        {
            var expiration = new ExpirationStrategyMock();
            var planId = new Guid(1, (short)0, (short)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0);
            int calledTimes = 0;

            var plan = LoadPlan(planId);
            var referenceRoute = LoadPlan(planId);

            Func<Guid, PlanNodeDO> cacheMiss = x =>
            {
                calledTimes++;
                Assert.AreEqual(planId, x);
                return plan;
            };

            var cache = new PlanCache(expiration);

            var plan1 = cache.Get(planId, cacheMiss);
            var updated = LoadPlan(planId, "updated");

            var o = new PlanSnapshot(plan1, false);
            var c = new PlanSnapshot(updated, false);

            cache.Update(updated.Id, c.Compare(o));
            var plan2 = cache.Get(planId, cacheMiss);

            Assert.AreEqual(1, calledTimes);
            Assert.IsTrue(AreEquals(plan1, referenceRoute));
            Assert.IsTrue(AreEquals(plan2, updated));
        }

        private static bool AreEquals(PlanNodeDO a, PlanNodeDO b)
        {
            var snapShotA = new PlanSnapshot(a, false);
            var snapShotB = new PlanSnapshot(b, false);

            return !snapShotB.Compare(snapShotA).HasChanges;
        }
    }
}
