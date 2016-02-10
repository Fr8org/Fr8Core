using System;
using System.Linq;
using Data.Entities;
using Data.Repositories.Plan;
using Hub.StructureMap;
using NUnit.Framework;
using UtilitiesTesting;

namespace DockyardTest.Repositories.Plan
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

        private RouteNodeDO LoadRoute(Guid arg, string postfix = "")
        {
            return new PlanDO
            {
                Id = arg,
                Name = "Route",
                ChildNodes =
                {
                    new ActivityDO()
                    {
                        Id = new Guid(2, (short) 0, (short) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0),
                        Name = "Base1"+postfix,
                    },
                    new ActivityDO()
                    {
                        Id = new Guid(3, (short) 0, (short) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0),
                        Name = "Base2"+postfix,
                    }
                }
            };
        }
        

        [Test]
        public void CanLoadOnCacheMiss()
        {
            var expiration = new ExpirationStrategyMock();
            var routeId = new Guid(1, (short) 0, (short) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0);
            int calledTimes = 0;

            Func<Guid, RouteNodeDO> cacheMiss = x =>
            {
                calledTimes++;
                Assert.AreEqual(routeId, x);
                return LoadRoute(x);
            };

            var cache = new PlanCache(expiration);

            cache.Get(routeId, cacheMiss);

            Assert.AreEqual(1, calledTimes);
        }

        [Test]
        public void CanExpireItems()
        {
            var expiration = new ExpirationStrategyMock();
            var routeId = new Guid(1, (short)0, (short)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0);
            int calledTimes = 0;

            Func<Guid, RouteNodeDO> cacheMiss = x =>
            {
                calledTimes++;
                Assert.AreEqual(routeId, x);
                return LoadRoute(x);
            };

            var cache = new PlanCache(expiration);


            cache.Get(routeId, cacheMiss);
            expiration.InvokeExpirationCallback();
            cache.Get(routeId, cacheMiss);

            Assert.AreEqual(2, calledTimes);
        }

        [Test]
        public void CanLoadFromCache()
        {
            var expiration = new ExpirationStrategyMock();
            var routeId = new Guid(1, (short)0, (short)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0);
            int calledTimes = 0;

            var route = LoadRoute(routeId);

            Func<Guid, RouteNodeDO> cacheMiss = x =>
            {
                calledTimes++;
                return route;
            };

            var cache = new PlanCache(expiration);

            var route1 = cache.Get(routeId, cacheMiss);
          
            Assert.AreEqual(route1, route);

            foreach (var id in RouteTreeHelper.Linearize(route).Select(x=>x.Id))
            {
                Assert.AreEqual(route, cache.Get(id, cacheMiss));
            }

            Assert.AreEqual(1, calledTimes);
        }

        [Test]
        public void CanLoadFromCacheUsingChildActionsId()
        {
            var expiration = new ExpirationStrategyMock();
            var routeId = new Guid(1, (short)0, (short)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0);
            int calledTimes = 0;

            var route = LoadRoute(routeId);

            Func<Guid, RouteNodeDO> cacheMiss = x =>
            {
                calledTimes++;
                return route;
            };

            var cache = new PlanCache(expiration);

            var route1 = cache.Get(new Guid(2, (short)0, (short)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0), cacheMiss);

            Assert.AreEqual(route1, route);

            foreach (var id in RouteTreeHelper.Linearize(route).Select(x => x.Id))
            {
                Assert.AreEqual(route, cache.Get(id, cacheMiss));
            }

            Assert.AreEqual(1, calledTimes);
        }


        [Test]
        public void CanUpdateCache()
        {
            var expiration = new ExpirationStrategyMock();
            var routeId = new Guid(1, (short)0, (short)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0);
            int calledTimes = 0;

            var route = LoadRoute(routeId);

            Func<Guid, RouteNodeDO> cacheMiss = x =>
            {
                calledTimes++;
                Assert.AreEqual(routeId, x);
                return route;
            };

            var cache = new PlanCache(expiration);

            var route1 = cache.Get(routeId, cacheMiss);
            var updated = LoadRoute(routeId, "updated");

            var o = new RouteSnapshot(route1, false);
            var c = new RouteSnapshot(updated, false);

            cache.Update(updated.Id, c.Compare(o));
            var route2 = cache.Get(routeId, cacheMiss);

            Assert.AreEqual(1, calledTimes);
            Assert.IsTrue(AreEquals(route1, route));
            Assert.IsTrue(AreEquals(route2, updated));
        }

        private static bool AreEquals(RouteNodeDO a, RouteNodeDO b)
        {
            var snapShotA = new RouteSnapshot(a, false);
            var snapShotB = new RouteSnapshot(b, false);

            return !snapShotB.Compare(snapShotA).HasChanges;
        }
    }
}
