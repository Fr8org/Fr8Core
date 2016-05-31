using System;
using System.Threading.Tasks;
using System.Web.Http;
using Hub.Infrastructure;
using Microsoft.AspNet.Identity;
using PlanDirectory.Infrastructure;
using PlanDirectory.Interfaces;
using StructureMap;

namespace PlanDirectory.Controllers.Api
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
        public async Task<IHttpActionResult> Post(PublishPlanTemplateDTO dto)
        {
            var fr8AccountId = User.Identity.GetUserId();
            await _planTemplate.CreateOrUpdate(fr8AccountId, dto);

            return Ok();
        }

        [HttpGet]
        [Fr8ApiAuthorize]
        [PlanDirectoryHMACAuthenticate]
        public async Task<IHttpActionResult> Get(Guid id)
        {
            var fr8AccountId = User.Identity.GetUserId();
            var planTemplateDTO = await _planTemplate.Get(fr8AccountId, id);

            return Ok(planTemplateDTO);
        }

        [HttpPost]
        public IHttpActionResult Search(
            string text, int? pageStart = null, int? pageSize = null)
        {
            // Commented out untill Azure Search Index activity is implemented.
            // var searchRequest = new SearchRequestDTO()
            // {
            //     Text = text,
            //     PageStart = pageStart.GetValueOrDefault(),
            //     PageSize = pageSize.GetValueOrDefault()
            // };
            // 
            // var searchResult = await _planTemplate.Search(searchRequest);

            return Ok(new PublishPlanTemplateDTO[] { });
        }
    }
}