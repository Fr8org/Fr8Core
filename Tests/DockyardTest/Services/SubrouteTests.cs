using System;
using NUnit.Framework;
using StructureMap;
using Data.Interfaces;
using Hub.Interfaces;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Services
{
    [TestFixture]
    [Category("Subroute")]
    public class SubrouteTests : BaseTest
    {
        private ISubroute _subroute;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _subroute = ObjectFactory.GetInstance<ISubroute>();
        }

        /*[Test]
        public void SubrouteService_CanCreate()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = FixtureData.TestRoute2();
                uow.RouteRepository.Add(plan);

                //add processnode to process
                var sampleNodeTemplate = FixtureData.TestSubrouteDO2();
                sampleNodeTemplate.ParentRouteNodeId = plan.Id;

                // Create
                _subroute.Store(uow, sampleNodeTemplate);
                //will throw exception if it fails

                if (uow.SubrouteRepository.GetByKey(sampleNodeTemplate.Id) == null)
                {
                    throw new Exception("Creating logic was passed a null SubrouteDO");
                }
            }
        }

        [Test]
        public void SubrouteService_CanUpdate()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = FixtureData.TestRoute2();
                uow.RouteRepository.Add(plan);

                //add processnode to process
                var sampleNodeTemplate = FixtureData.TestSubrouteDO2();
                sampleNodeTemplate.ParentRouteNodeId = plan.Id;


                // Create
                _subroute.Store(uow, sampleNodeTemplate);
                //will throw exception if it fails

                sampleNodeTemplate.Name = "UpdateTest";

                // Update
                _subroute.Update(uow, sampleNodeTemplate);
                //will throw exception if it fails

                if (uow.SubrouteRepository.GetByKey(sampleNodeTemplate.Id).Name != "UpdateTest")
                {
                    throw new Exception("SubrouteDO updating logic was failed.");
                }
            }
        }

        // MockDB has boken logic when working with collections of objects of derived types
        // We add object to SubrouteRepository but Delete logic recusively traverse Activity repository.
        [Ignore("MockDB behavior is incorrect")]
        [Test]
        public void SubrouteService_CanDelete()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = FixtureData.TestRoute2();
                uow.RouteRepository.Add(plan);

                //add processnode to process
                var sampleNodeTemplate = FixtureData.TestSubrouteDO2();
                sampleNodeTemplate.ParentRouteNodeId = plan.Id;

                // Create
                _subroute.Store(uow, sampleNodeTemplate);
                //will throw exception if it fails

                if (uow.RouteNodeRepository.GetByKey(sampleNodeTemplate.Id) == null)
                {
                    throw new Exception("SubrouteDO add logic was failed.");
                }

                // Delete
                _subroute.Delete(uow, sampleNodeTemplate.Id);
                //will throw exception if it fails

                if (uow.SubrouteRepository.GetByKey(sampleNodeTemplate.Id) != null)
                {
                    throw new Exception("SubrouteDO deleting logic was failed.");
                }
            }
        }
        */
    }
}