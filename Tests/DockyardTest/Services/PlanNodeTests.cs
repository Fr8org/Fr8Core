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
    [Category("PlanNode")]
    public class PlanNodeTests : BaseTest
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            FixtureData.AddTestActivityTemplate();
        }

        [Test]
        public void GetAvailableData_ShouldReturnFields()
        {
            var plan = new PlanDO();
            plan.Name = "sdfasdfasdf";
            plan.PlanState = PlanState.Active;
            var testActionTree = FixtureData.TestActivity2Tree();

            plan.ChildNodes.Add(testActionTree);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.PlanRepository.Add(plan);
                uow.SaveChanges();
            }

            IPlanNode planNodeService = ObjectFactory.GetInstance<IPlanNode>();
            var fieldsCrate = planNodeService.GetAvailableData(testActionTree.ChildNodes.Last().Id, CrateDirection.Upstream, AvailabilityType.NotSet);
            Assert.NotNull(fieldsCrate);
            Assert.NotNull(fieldsCrate.AvailableFields);
            Assert.AreEqual(66, fieldsCrate.AvailableFields.Count);
        }
    }
}
