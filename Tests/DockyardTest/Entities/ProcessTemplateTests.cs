using System.Linq;
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
                var route = FixtureData.TestRoute2();
                uow.RouteRepository.Add(route);

                var subroute = FixtureData.TestSubrouteDO2();
                uow.SubrouteRepository.Add(subroute);
                route.StartingSubroute = subroute;

                uow.SaveChanges();

                var result = uow.RouteRepository.GetQuery()
                    .SingleOrDefault(pt => pt.StartingSubrouteId == subroute.Id);

                Assert.AreEqual(subroute.Id, result.StartingSubroute.Id);
                Assert.AreEqual(subroute.Name, result.StartingSubroute.Name);
            }
        }

        [Test]
        public void GetStandardEventSubscribers_ReturnsRoutes()
        {
            FixtureData.TestRouteWithSubscribeEvent();
            IRoute curRoute = ObjectFactory.GetInstance<IRoute>();
            EventReportCM curEventReport = FixtureData.StandardEventReportFormat();

            var result = curRoute.GetMatchingRoutes("testuser1", curEventReport);

            Assert.IsNotNull(result);
            Assert.Greater(result.Count, 0);
            Assert.Greater(result.Where(name => name.Name.Contains("StandardEventTesting")).Count(), 0);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(System.ArgumentNullException))]
        public void GetStandardEventSubscribers_UserIDEmpty_ThrowsException()
        {
            IRoute curRoute = ObjectFactory.GetInstance<IRoute>();

            curRoute.GetMatchingRoutes("", new EventReportCM());
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(System.ArgumentNullException))]
        public void GetStandardEventSubscribers_EventReportMSNULL_ThrowsException()
        {
            IRoute curRoute = ObjectFactory.GetInstance<IRoute>();

            curRoute.GetMatchingRoutes("UserTest", null);
        }


    }
}