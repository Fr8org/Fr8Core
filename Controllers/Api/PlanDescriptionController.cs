using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Infrastructure.StructureMap;
using Hub.Interfaces;
using System.Net.Http;
using System.Security.Claims;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity;
using HubWeb.Infrastructure;

namespace HubWeb.Controllers
{
    [Fr8ApiAuthorize]
    [RoutePrefix("plan_descriptions")]
    public class PlanDescriptionController : Fr8BaseApiController
    {
        private IPlanDescription _planDescription;

        public PlanDescriptionController()
        {
            _planDescription = ObjectFactory.GetInstance<IPlanDescription>();
        }

        [HttpPost]
#if DEBUG
        [AllowAnonymous]
#endif
        public async Task<IHttpActionResult> Post(Guid planId, string userId)
        {
            var result = _planDescription.Save(planId, userId);
            return Ok(result);
        }



        [HttpPost]
#if DEBUG
        [AllowAnonymous]
#endif
        public async Task<IHttpActionResult> Create(int planDescriptionId, string userId)
        {
            var result = _planDescription.BuildAPlan(planDescriptionId, userId);
            return Ok(result);
        }


        [HttpGet]
#if DEBUG
        [AllowAnonymous]
#endif
        public async Task<IHttpActionResult> Get(string userId)
        {
            var result = _planDescription.GetDescriptions(userId);
            return Ok(result);
        }
    }
}