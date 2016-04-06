using System.Web.Http;
using Data.Entities;
using System.Collections.Generic;
using Data.States;
using Utilities.Configuration.Azure;
using System.Web.Http.Description;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;

namespace terminalBox.Controllers
{
    [RoutePrefix("terminals")]
    public class TerminalController : ApiController
    {
        [HttpGet]
        [Route("discover")]
        [ResponseType(typeof(StandardFr8TerminalCM))]
        public IHttpActionResult DiscoverTerminals()
        {
            var terminal = new TerminalDTO()
            {
                Name = "terminalBox",
                TerminalStatus = TerminalStatus.Active,
                Endpoint = CloudConfigurationManager.GetSetting("terminalBox.TerminalEndpoint"),
                Version = "1",
                AuthenticationType = AuthenticationType.External
            };

            var webService = new WebServiceDTO
            {
                Name = "Box",
                IconPath = "/Content/icons/web_services/dropbox-icon-64x64.png"
            };

            StandardFr8TerminalCM curStandardFr8TerminalCM = new StandardFr8TerminalCM()
            {
                Definition = terminal
            };

            return Json(curStandardFr8TerminalCM);
        }
    }
}