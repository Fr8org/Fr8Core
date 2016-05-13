using System;
using System.Threading.Tasks;
using System.Web.Http;
using StructureMap;
using Hub.Infrastructure;
using Hub.Interfaces;

namespace HubWeb.Controllers
{
    [Fr8ApiAuthorize]
    public class PlanTemplatesController : Fr8BaseApiController
    {
        private IPlanTemplates _planTemplates;

        public PlanTemplatesController()
        {
            _planTemplates = ObjectFactory.GetInstance<IPlanTemplates>();
        }

        [HttpPost]
        public async Task Delete(int id, string userId)
        {
            _planTemplates.DeleteTemplate(id, userId);
        }

        [HttpPost]
        public async Task<IHttpActionResult> Post(Guid planId, string userId)
        {
            var result = _planTemplates.SavePlan(planId, userId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IHttpActionResult> Create(int id, string userId)
        {
            var result = _planTemplates.LoadPlan(id, userId);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IHttpActionResult> Get(int id, string userId)
        {
            var result = _planTemplates.GetTemplate(id, userId);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetAll(string userId)
        {
            var result = _planTemplates.GetTemplates(userId);
            return Ok(result);
        }
    }
}