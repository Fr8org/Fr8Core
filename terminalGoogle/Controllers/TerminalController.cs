using System.Web.Http;
using System.Web.Http.Description;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Services;

namespace terminalGoogle.Controllers
{
    //[RoutePrefix("terminals")]
    public class TerminalController : ApiController
    {
        private readonly IActivityStore _activityStore;

        public TerminalController(IActivityStore activityStore)
        {
            _activityStore = activityStore;
        }

        /// <summary>
        /// Plugin discovery infrastructure.
        /// Action returns list of supported actions by plugin.
        /// </summary>
        [HttpGet]
        [Route("discover")]
        [ResponseType(typeof(StandardFr8TerminalCM))]
        public IHttpActionResult DiscoverTerminals()
        {
            StandardFr8TerminalCM curStandardFr8TerminalCM = new StandardFr8TerminalCM()
            {
                Definition = TerminalData.TerminalDTO,
                Activities = _activityStore.GetAllActivities(TerminalData.TerminalDTO)
            };
            return Json(curStandardFr8TerminalCM);
        }
    }
}