using System.Linq;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Core.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using Web.Controllers;

namespace DockyardTest.Entities
{
    [TestFixture]
    [Category("Route")]
    public class RouteTests : BaseTest
    {
        [Test]
        public void Route_ShouldBeAssignedStartingProcessNodeTemplate()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var route = FixtureData.TestRoute2();
                uow.RouteRepository.Add(route);

                var processNodeTemplate = FixtureData.TestProcessNodeTemplateDO2();
                uow.ProcessNodeTemplateRepository.Add(processNodeTemplate);
                route.StartingProcessNodeTemplate = processNodeTemplate;

                uow.SaveChanges();

                var result = uow.RouteRepository.GetQuery()
                    .SingleOrDefault(pt => pt.StartingProcessNodeTemplateId == processNodeTemplate.Id);

                Assert.AreEqual(processNodeTemplate.Id, result.StartingProcessNodeTemplate.Id);
                Assert.AreEqual(processNodeTemplate.Name, result.StartingProcessNodeTemplate.Name);
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