using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Services;

namespace Fr8.TerminalBase.BaseClasses
{
    /// <summary>
    /// Base class for Web API controller that are intended to process terminal related requests from the Hub. 
    /// See https://github.com/Fr8org/Fr8Core/blob/dev/Docs/ForDevelopers/SDK/.NET/Reference/DefaultTerminalController.md
    /// </summary>
    public abstract class DefaultTerminalController : ApiController
    {
        private readonly IActivityStore _activityStore;
        private readonly IHubDiscoveryService _hubDiscovery;

        protected DefaultTerminalController(IActivityStore activityStore, IHubDiscoveryService hubDiscovery)
        {
            _activityStore = activityStore;
            _hubDiscovery = hubDiscovery;
        }

        [HttpGet]
        [Route("discover")]
        [ResponseType(typeof(StandardFr8TerminalCM))]
        public IHttpActionResult Get()
        {
            StandardFr8TerminalCM curStandardFr8TerminalCM = new StandardFr8TerminalCM
            {
                Definition = _activityStore.Terminal,
                Activities = _activityStore.GetAllTemplates()
            };

            if (Request.Headers.Contains("Fr8HubCallBackUrl") && Request.Headers.Contains("Fr8HubCallbackSecret"))
            {
                var hubUrl = Request.Headers.GetValues("Fr8HubCallBackUrl").First();
                var secret = Request.Headers.GetValues("Fr8HubCallbackSecret").First();

                _hubDiscovery.SetHubSecret(hubUrl, secret);
            }

            return Json(curStandardFr8TerminalCM);
        }
    }
}
