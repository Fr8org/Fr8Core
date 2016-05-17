using System.Collections.Generic;
using System.Web.Http;
using Data.States;
using Utilities.Configuration.Azure;
using System.Web.Http.Description;
using Fr8Data.Constants;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using TerminalBase.Services;

namespace terminalDocuSign.Controllers
{
    [RoutePrefix("terminals")]
    public class TerminalController : ApiController
    {
        [HttpGet]
        [Route("discover")]
        [ResponseType(typeof(StandardFr8TerminalCM))]
        public IHttpActionResult Get()
        {
            var curStandardFr8TerminalCM = new StandardFr8TerminalCM()
            {
                Definition = TerminalData.TerminalDTO,
                Activities = ActivityStore.GetAllActivities()
            };

            return Json(curStandardFr8TerminalCM);
        }
    }
}