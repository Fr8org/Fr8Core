using System.Web.Http;
using System.Collections.Generic;
using Utilities.Configuration.Azure;
using System.Web.Http.Description;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using TerminalBase.Services;

namespace terminalAtlassian.Controllers
{
    [RoutePrefix("terminals")]
    public class TerminalController : ApiController
    {
        [HttpGet]
        [Route("discover")]
        [ResponseType(typeof(StandardFr8TerminalCM))]
        public IHttpActionResult Get()
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