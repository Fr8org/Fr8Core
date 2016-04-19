using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Google.GData.Extensions;
using Utilities.Configuration.Azure;

namespace terminalGoogle.Controllers
{
    [RoutePrefix("terminals")]
    public class TerminalController : ApiController
    {
        /// <summary>
        /// Plugin discovery infrastructure.
        /// Action returns list of supported actions by plugin.
        /// </summary>
        [HttpGet]
        [Route("discover")]
        [ResponseType(typeof(StandardFr8TerminalCM))]
        public IHttpActionResult DiscoverTerminals()
        {
            var webService = new WebServiceDTO
            {
                Name = "Google",
                IconPath= "/Content/icons/web_services/google-icon-64x64.png"
            };

            var terminal = new TerminalDTO()
            {
                Endpoint = CloudConfigurationManager.GetSetting("terminalGoogle.TerminalEndpoint"),
                TerminalStatus = TerminalStatus.Active,
                Name = "terminalGoogle",
                Label = "Google",
                Version = "1",
                AuthenticationType = AuthenticationType.External
            };

            var extractDataAction = new ActivityTemplateDTO
            {
                Name = "Get_Google_Sheet_Data",
                Label = "Get Google Sheet Data",
                Version = "1",
                Category = ActivityCategory.Receivers,
                Terminal = terminal,
                NeedsAuthentication = true,
                MinPaneWidth = 300,
                WebService = webService,
                Tags = "Table Data Generator"
            };

            var receiveGoogleForm = new ActivityTemplateDTO
            {
                Name = "Monitor_Form_Responses",
                Label = "Monitor Form Responses",
                Version = "1",
                Category = ActivityCategory.Monitors,
                Terminal = terminal,
                NeedsAuthentication = true,
                WebService = webService,
                MinPaneWidth = 300
            };

            var saveDataAction = new ActivityTemplateDTO
            {
                Name = "Save_To_Google_Sheet",
                Label = "Save To Google Sheet",
                Version = "1",
                Category = ActivityCategory.Forwarders,
                Terminal = terminal,
                NeedsAuthentication = true,
                MinPaneWidth = 300,
                WebService = webService
            };

            return Json(new StandardFr8TerminalCM()
            {
                Definition = terminal,
                Activities = new List<ActivityTemplateDTO>
                {
                    extractDataAction,
                    receiveGoogleForm,
                    saveDataAction
                }
            });    
        }
    }
}