using System.Web.Http;
using System.Web.Http.Description;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Services;

namespace Fr8.TerminalBase.BaseClasses
{
    public class DefaultTerminalController : ApiController
    {
        private readonly IActivityStore _activityStore;

        public DefaultTerminalController(IActivityStore activityStore)
        {
            _activityStore = activityStore;
        }

        [HttpGet]
        [Route("discover")]
        [ResponseType(typeof(StandardFr8TerminalCM))]
        public IHttpActionResult Get()
        {
            StandardFr8TerminalCM curStandardFr8TerminalCM = new StandardFr8TerminalCM
            {
                Definition = _activityStore.Terminal,
                Activities = _activityStore.GetAllActivities()
            };

            return Json(curStandardFr8TerminalCM);
        }
    }
}
