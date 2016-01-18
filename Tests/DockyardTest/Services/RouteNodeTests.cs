using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        }

        [Test]
        public void GetDesignTimeFieldsByDirectionTerminal_ShouldGenerateCorrectDesigntimeURL()
        {
            var _restfulServiceClient = new Mock<IRestfulServiceClient>();
            _restfulServiceClient.Setup(r => r.GetAsync<StandardDesignTimeFieldsCM>(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()));
            ObjectFactory.Configure(cfg => cfg.For<IRestfulServiceClient>().Use(_restfulServiceClient.Object));
            IRouteNode _routeNodeService = ObjectFactory.GetInstance<IRouteNode>();

            Guid id = Guid.NewGuid();
            CrateDirection direction = CrateDirection.Downstream;
            AvailabilityType availability = AvailabilityType.RunTime;

            string resultUrl = String.Format(
                "http://localhost:30643/api/v1/routenodes/designtime_fields_dir?id={0}&direction={1}&availability={2}",
                id.ToString(),
                ((int)direction).ToString(),
                ((int)availability).ToString());
            _routeNodeService.GetDesignTimeFieldsByDirectionTerminal(id, direction, availability);

            _restfulServiceClient.Verify(o => o.GetAsync<StandardDesignTimeFieldsCM>(It.Is<Uri>(p => p.ToString() == resultUrl), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()));
        }

        [Test]
        public void GetDesignTimeFieldsByDirection_ShouldReturnDesignTimeFieldsCrate()
        {
            var testActionTree = FixtureData.TestActionTree();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.RouteNodeRepository.Add(testActionTree);
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
