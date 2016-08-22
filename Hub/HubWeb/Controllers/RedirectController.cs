using System;
using System.Threading.Tasks;
using StructureMap;
using System.Web.Mvc;
using Fr8.Infrastructure.Utilities;
using Hub.Interfaces;
using Hub.Managers;

namespace HubWeb.Controllers
{
    public class RedirectController : Controller
    {
        private readonly IPlan _plan;

        public RedirectController()
        {
            _plan = ObjectFactory.GetInstance<IPlan>();
        }

        public ActionResult TwilioSMS()
        {
            var configRepository = ObjectFactory.GetInstance<IConfigRepository>();

            string smsURL = configRepository.Get("DocumentationFr8Site_SMSLink");
            return Redirect(smsURL);
        }


        [HttpGet]
        [DockyardAuthorize]
        public async Task<ActionResult> ClonePlan(Guid id)
        {
            //let's clone the plan and redirect user to that cloned plan url
            var clonedPlan = _plan.Clone(id);
            var baseUri = Request.Url.GetLeftPart(UriPartial.Authority);
            var clonedPlanUrl = baseUri + "/dashboard/plans/" + clonedPlan.Id + "/builder?viewMode=kiosk&view=Collection";
            return View("~/Views/Redirect/ClonePlan.cshtml", null, clonedPlanUrl);
        }
    }
}