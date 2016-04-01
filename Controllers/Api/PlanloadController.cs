using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hangfire;
using Hub.Interfaces;
using Hub.Managers;
using HubWeb.Infrastructure;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;
using Utilities.Configuration.Azure;
using Utilities.Interfaces;

namespace HubWeb.Controllers
{
    public class PlanloadController : ApiController
    {
        private readonly Hub.Interfaces.IPlan _plan;
        private readonly IPusherNotifier _pusherNotifier;

        public PlanloadController()
        {
            _plan = ObjectFactory.GetInstance<IPlan>();
        }

        [HttpGet]
        [Fr8ApiAuthorize]
        public async Task<IHttpActionResult> Get(Guid id)
        {
            //let's clone the plan and redirect user to that cloned plan url
            var clonedPlan = await _plan.CloneById(id);
            var baseUri = Request.RequestUri.GetLeftPart(UriPartial.Authority);
            var clonedPlanUrl = baseUri +
            "/dashboard/plans/" + clonedPlan.Id + "/builder?viewMode=kiosk&view=Collection";

            


            return Redirect(clonedPlanUrl);
        }

    }
}