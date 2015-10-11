using System.Web.Http.Results;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using DockyardTest.Controllers.Api;
using UtilitiesTesting.Fixtures;
using Web.Controllers;

namespace DockyardTest.Controllers
{
    public class CriteriaControllerTest : ApiControllerTestBase
    {
        private ProcessNodeTemplateDO _curProcessNodeTemplate;
        private CriteriaDO _curCriteria;

        public override void SetUp()
        {
            base.SetUp();
            InitializeCriteria();
        }

        [Test]
        [Category("CriteriaController.GetByProcessNodeTemplateId")]
        public void CriteriaController_GetByProcessNodeTemplateId()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var controller = CreateController<CriteriaController>();

                var actionResult = controller.GetByProcessNodeTemplateId(_curProcessNodeTemplate.Id);

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
                var curProcessTemplate = FixtureData.TestProcessTemplate1();
                uow.ProcessTemplateRepository.Add(curProcessTemplate);
                uow.SaveChanges();

                //Add a template
                var processTemplate = FixtureData.TestProcessTemplate1();
                uow.ProcessTemplateRepository.Add(processTemplate);
                uow.SaveChanges();
                //Add a processnodetemplate to processtemplate 
                _curProcessNodeTemplate = FixtureData.TestProcessNodeTemplateDO1();
                _curProcessNodeTemplate.ParentActivityId = processTemplate.Id;
                uow.ProcessNodeTemplateRepository.Add(_curProcessNodeTemplate);
                uow.SaveChanges();

                /*_curProcessNodeTemplate = FixtureData.TestProcessNodeTemplateDO1();
                uow.ProcessNodeTemplateRepository.Add(_curProcessNodeTemplate);
                uow.SaveChanges();*/

                _curCriteria = FixtureData.TestCriteria1();
                _curCriteria.ProcessNodeTemplate = _curProcessNodeTemplate;

                uow.CriteriaRepository.Add(_curCriteria);
                uow.SaveChanges();
            }
        }
    }
}
