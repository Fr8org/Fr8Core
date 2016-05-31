using System;
using System.Threading.Tasks;
using System.Web.Http;
using StructureMap;
using Hub.Infrastructure;
using Microsoft.AspNet.Identity;
using PlanDirectory.Infrastructure;
using PlanDirectory.Interfaces;

namespace PlanDirectory.Controllers
{
    public class PlanTemplatesController : ApiController
    {
        private readonly IPlanTemplate _planTemplate;
        private readonly ISearchProvider _searchProvider;


        public PlanTemplatesController()
        {
            _planTemplate = ObjectFactory.GetInstance<IPlanTemplate>();
            _searchProvider = ObjectFactory.GetInstance<ISearchProvider>();
        }

        [HttpPost]
        [Fr8ApiAuthorize]
        [PlanDirectoryHMACAuthenticate]
        public async Task<IHttpActionResult> Post(PlanTemplateDTO dto)
        {
            var fr8AccountId = User.Identity.GetUserId();
            await _planTemplate.CreateOrUpdate(fr8AccountId, dto);
            await _searchProvider.CreateOrUpdate(dto);

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

        [HttpGet]
        public async Task<IHttpActionResult> Search(
            string text, int? pageStart = null, int? pageSize = null)
        {
            var searchRequest = new SearchRequestDTO()
            {
                Text = text,
                PageStart = pageStart.GetValueOrDefault(),
                PageSize = pageSize.GetValueOrDefault()
            };
            
            var searchResult = await _searchProvider.Search(searchRequest);

            return Ok(searchResult);
        }
    }
}