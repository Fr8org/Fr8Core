using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hub.Interfaces;
using NUnit.Framework;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Hub.Managers;
using StructureMap;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Moq;
using Data.Interfaces.Manifests;
using Data.Interfaces;

namespace DockyardTest.Services
{
    [TestFixture]
    [Category("RouteNode")]
    public class RouteNodeTests : BaseTest
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            FixtureData.AddTestActivityTemplate();
        }

        [Test]
        public void GetDesignTimeFieldsByDirection_ShouldReturnDesignTimeFieldsCrate()
        {
            var route = new PlanDO();
            route.Name = "sdfasdfasdf";
            route.RouteState = RouteState.Active;
            var testActionTree = FixtureData.TestActivity2Tree();
            
            route.ChildNodes.Add(testActionTree);
            
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.PlanRepository.Add(route);
                uow.SaveChanges();
            }

            IRouteNode _routeNodeService = ObjectFactory.GetInstance<IRouteNode>();
            var fieldsCrate = _routeNodeService.GetDesignTimeFieldsByDirection(testActionTree.ChildNodes.Last().Id, CrateDirection.Upstream, AvailabilityType.NotSet);
            Assert.NotNull(fieldsCrate);
            Assert.NotNull(fieldsCrate.Fields);
            Assert.IsInstanceOfType(typeof(StandardDesignTimeFieldsCM), fieldsCrate);
            Assert.AreEqual(66, fieldsCrate.Fields.Count());
        }
    }
}
