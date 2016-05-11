using System.Web.Http;
using StructureMap;
using Hub.Infrastructure;
using PlanDirectory.Infrastructure;
using PlanDirectory.Interfaces;

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
        [Fr8ApiAuthorize]
        [PlanDirectoryHMACAuthenticate]
        public IHttpActionResult Publish(PublishPlanTemplateDTO planTemplate)
        {
            _planTemplate.Publish(planTemplate);
            return Ok();
        }

        [HttpPost]
        [Fr8ApiAuthorize]
        [PlanDirectoryHMACAuthenticate]
        public IHttpActionResult Unpublish(PublishPlanTemplateDTO planTemplate)
        {
            _planTemplate.Unpublish(planTemplate);
            return Ok();
        }

        [HttpPost]
        public IHttpActionResult Search(string text)
        {
            throw new System.NotImplementedException();
        }
    }
}