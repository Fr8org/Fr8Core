using System.Linq;
using Data.Entities;
using NUnit.Framework;
using StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;
using HubWeb.Controllers;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Data.Interfaces.Manifests;

namespace DockyardTest.Entities
{
    [TestFixture]
    [Category("Route")]
    public class RouteTests : BaseTest
    {
        [Test]
        public void Route_ShouldBeAssignedStartingSubroute()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = FixtureData.TestRoute2();
                var subroute = FixtureData.TestSubrouteDO2();

                plan.ChildNodes.Add(subroute);
                uow.PlanRepository.Add(plan);
                plan.StartingSubroute = subroute;

                uow.SaveChanges();

                var result = uow.PlanRepository.GetById<PlanDO>(plan.Id);//.SingleOrDefault(pt => pt.StartingSubrouteId == subroute.Id);

                Assert.AreEqual(subroute.Id, result.StartingSubroute.Id);
                Assert.AreEqual(subroute.Name, result.StartingSubroute.Name);
            }
        }

        [Test]
        public void GetStandardEventSubscribers_ReturnsRoutes()
        {
            FixtureData.TestRouteWithSubscribeEvent();
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