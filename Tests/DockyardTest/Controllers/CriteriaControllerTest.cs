using System.Web.Http.Results;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using HubWeb.Controllers;
using DockyardTest.Controllers.Api;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Controllers
{
    public class CriteriaControllerTest : ApiControllerTestBase
    {
        private SubrouteDO _curSubroute;
        private CriteriaDO _curCriteria;

        public override void SetUp()
        {
            base.SetUp();
            InitializeCriteria();
        }

        [Test]
        public void CriteriaController_GetBySubrouteId()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var controller = CreateController<CriteriaController>();

                var actionResult = controller.BySubrouteId(_curSubroute.Id);

                var okResult = actionResult as OkNegotiatedContentResult<CriteriaDTO>;

                Assert.IsNotNull(okResult);
                Assert.IsNotNull(okResult.Content);
                Assert.AreEqual(okResult.Content.Id, _curCriteria.Id);
            }
        }

        private void InitializeCriteria()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {

               
                //Add a template
                var plan = FixtureData.TestRoute1();
                uow.PlanRepository.Add(plan);
                //Add a processnodetemplate to plan 
                _curSubroute = FixtureData.TestSubrouteDO1();
                _curSubroute.ParentRouteNodeId = plan.Id;
                plan.ChildNodes.Add(_curSubroute);
                
                uow.SaveChanges();

                /*_curSubroute = FixtureData.TestSubrouteDO1();
                uow.SubrouteRepository.Add(_curSubroute);
                uow.SaveChanges();*/

                _curCriteria = FixtureData.TestCriteria1();
                _curCriteria.Subroute = _curSubroute;

                uow.CriteriaRepository.Add(_curCriteria);
                uow.SaveChanges();
            }
        }
    }
}
