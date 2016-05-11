using System.Threading.Tasks;
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
        public async Task<IHttpActionResult> Publish(PublishPlanTemplateDTO planTemplate)
        {
            await _planTemplate.Publish(planTemplate);
            return Ok();
        }

        [HttpPost]
        [Fr8ApiAuthorize]
        [PlanDirectoryHMACAuthenticate]
        public async Task<IHttpActionResult> Unpublish(PublishPlanTemplateDTO planTemplate)
        {
            await _planTemplate.Unpublish(planTemplate);
            return Ok();
        }

        [HttpPost]
        public async Task<IHttpActionResult> Search(string text)
        {
            var searchResult = await _planTemplate.Search(text);
            return Ok(searchResult);
        }
    }
}