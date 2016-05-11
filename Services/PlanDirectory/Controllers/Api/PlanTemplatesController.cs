using System.Web.Http;
using AutoMapper;
using StructureMap;
using Fr8Data.DataTransferObjects.PlanTemplates;
using PlanDirectory.Infrastructure;

namespace PlanDirectory.Controllers
{
    public class PlanTemplatesController : ApiController
    {
        private readonly IPlanTemplate _planTemplate;


        public PlanTemplatesController()
        {
            _planTemplate = ObjectFactory.GetInstance<IPlanTemplate>();
        }

        [HttpPost]
        public IHttpActionResult Publish(PlanTemplateDTO planTemplate)
        {
            // var planTemplateCM = Mapper.Map<PlanTemplateCM>(planTemplate);
            // _planTemplate.Publish(planTemplateCM);

            return Ok();
        }

        [HttpPost]
        public IHttpActionResult Unpublish(PlanTemplateDTO planTemplate)
        {
            // var planTemplateCM = Mapper.Map<PlanTemplateCM>(planTemplate);
            // _planTemplate.Unpublish(planTemplateCM);

            return Ok();
        }
    }
}