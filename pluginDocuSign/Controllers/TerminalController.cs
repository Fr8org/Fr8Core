using Data.Entities;
using Data.States;
using System.Collections.Generic;
using System.Web.Http;

namespace terminal_DocuSign.Controllers
{
    [RoutePrefix("terminals")]
    public class TerminalController : ApiController
    {
        [HttpGet]
        [Route("discover")]
        public IHttpActionResult Get()
        {
            var terminal = new PluginDO()
            {
                Name = "terminal_DocuSign",
                PluginStatus = PluginStatus.Active,
                Endpoint = "localhost:53234",
                RequiresAuthentication = true,
                Version = "1"
            };

            var waitForDocusignEventActionTemplate = new ActivityTemplateDO()
            {
                Version = "1",
                Name = "Wait_For_DocuSign_Event",
                Category = ActivityCategory.fr8_Monitor,
                Plugin = terminal
            };

			var sendDocuSignEnvelopeActionTemplate = new ActivityTemplateDO()
			{
				Version = "1",
				Name = "Send_DocuSign_Envelope",
                Category = ActivityCategory.fr8_Forwarder,
				Plugin = terminal
			};

            var extractDataFromEnvelopeActionTemplate = new ActivityTemplateDO()
            {
                Version = "1",
                Name = "Extract_From_DocuSign_Envelope",
                Category = ActivityCategory.fr8_Receiver,
                Plugin = terminal
            };

            var actionList = new List<ActivityTemplateDO>()
            {
                waitForDocusignEventActionTemplate,
                extractDataFromEnvelopeActionTemplate,
                sendDocuSignEnvelopeActionTemplate
            };

            return Ok(actionList);
        }
    }
}