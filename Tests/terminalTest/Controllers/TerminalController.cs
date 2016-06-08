using System.Web.Http.Description;
using System.Web.Http;
using fr8.Infrastructure.Data.Manifests;
using TerminalBase.Services;

namespace terminalTest.Controllers
{
    // This terminal contains activities that simplify core logic manual testing
    [RoutePrefix("terminals")]
    public class TerminalController : ApiController
    {
        /// <summary>
        /// Terminal discovery infrastructure.
        /// Action returns list of supported actions by terminal.

        /// </summary>
        [HttpGet]
        [Route("discover")]
        [ResponseType(typeof(StandardFr8TerminalCM))]

        public IHttpActionResult DiscoverTerminals()
        {
            StandardFr8TerminalCM curStandardFr8TerminalCM = new StandardFr8TerminalCM()
            {
                Definition = TerminalData.TerminalDTO,
                Activities = ActivityStore.GetAllActivities(TerminalData.TerminalDTO)
            };

            return Json(curStandardFr8TerminalCM);
        }
    }
}