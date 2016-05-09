using System.Web.Http.Results;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Fr8Data.DataTransferObjects;
using HubWeb.Controllers;
using HubTests.Controllers.Api;
using UtilitiesTesting.Fixtures;

namespace HubTests.Controllers
{
    public class CriteriaControllerTest : ApiControllerTestBase
    {
        private SubPlanDO _curSubPlan;
        private CriteriaDO _curCriteria;

        public override void SetUp()
        {
            base.SetUp();
            InitializeCriteria();
        }

        [Test]
        public void CriteriaController_ShouldHaveFr8ApiAuthorize()
        {
            ShouldHaveFr8ApiAuthorize(typeof(CriteriaController));
        }

        [Test]
        public void CriteriaController_GetBySubPlanId()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var controller = CreateController<CriteriaController>();

                var actionResult = controller.BySubPlanId(_curSubPlan.Id);

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
                var plan = FixtureData.TestPlan1();
                uow.PlanRepository.Add(plan);
                //Add a processnodetemplate to plan 
                _curSubPlan = FixtureData.TestSubPlanDO1();
                _curSubPlan.ParentPlanNodeId = plan.Id;
                plan.ChildNodes.Add(_curSubPlan);
                
                uow.SaveChanges();

                /*_curSubroute = FixtureData.TestSubrouteDO1();
                uow.SubrouteRepository.Add(_curSubroute);
                uow.SaveChanges();*/

                _curCriteria = FixtureData.TestCriteria1();
                _curCriteria.SubPlan = _curSubPlan;

                uow.CriteriaRepository.Add(_curCriteria);
                uow.SaveChanges();
            }
        }
    }
}
