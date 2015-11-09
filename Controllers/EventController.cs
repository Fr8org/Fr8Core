using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Web.Http;
using Data.Constants;
using Data.Crates;
using StructureMap;
using Data.Crates.Helpers;
using Data.Infrastructure;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Hub.Services;
using Newtonsoft.Json;

namespace HubWeb.Controllers
{
    /// <summary>
    /// Central logging Events controller
    /// </summary>
    public class EventController : ApiController
    {
        private readonly IEvent _event;
        private readonly ICrateManager _crate;
      

        private delegate void EventRouter(LoggingDataCm loggingDataCm);

        public EventController()
        {
            _event = ObjectFactory.GetInstance<IEvent>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
            
        }

        private EventRouter GetEventRouter(EventCM eventCm)
        {
            if (eventCm.EventName.Equals("Terminal Incident"))
            {
                return _event.HandleTerminalIncident;
            }

            if (eventCm.EventName.Equals("Terminal Event"))
            {
                return _event.HandleTerminalEvent;
            }

            throw new InvalidOperationException("Unknown EventDTO with name: " + eventCm.EventName);
        }

        [HttpPost]
        public IHttpActionResult Post(CrateDTO submittedEventsCrate)
        {
            var eventCm = _crate.FromDto(submittedEventsCrate).Get<EventCM>();

            if (eventCm.CrateStorage == null)
            {
                return Ok();
            }

            //Request of alex to keep things simple for now
            if (eventCm.CrateStorage.Count != 1)
            {
                throw new InvalidOperationException("Only single crate can be processed for now.");
            }

            EventRouter currentRouter = GetEventRouter(eventCm);

            var errorMsgList = new List<string>();
            foreach (var crateDTO in eventCm.CrateStorage)
            {
                if (crateDTO.ManifestType.Id != (int)MT.LoggingData)
                {
                    errorMsgList.Add("Don't know how to process an EventReport with the Contents: " +  JsonConvert.SerializeObject(_crate.ToDto(crateDTO)));
                    continue;
                }

                var loggingData = crateDTO.Get<LoggingDataCm>();
                currentRouter(loggingData);
            }

            if (errorMsgList.Count > 0)
            {
                throw new InvalidOperationException(String.Join(";;;", errorMsgList));
            }

            return Ok();
            
        }

        /*
         * Commented out as it is not clear in the spec.
         * It is mentioned in the spec that at the time of Activation, 
         * the action should query for the endpoint and add terminal name and its version with it.
         * But in code review got a comment about the purpose of this method. Need to discuss and clarify.
         */
        ///// <summary>
        ///// At activation time, actions should know the end point details to register with external services.
        ///// This method returns the end point URL that handles the Event Notifications at Dockyard.
        ///// </summary>
        //[HttpGet]
        //[Route("events/endpoint")]
        //public string EventNotificationEndPoint()
        //{
        //    //In development environment, Please uncomment this line.
        //    return "http://localhost:30643/events";

        //    //return "http://dockyard.company/events";
        //}

        [HttpPost]
        [Route("events")]
        public async Task<IHttpActionResult> ProcessIncomingEvents(
            [FromUri(Name = "dockyard_terminal")] string terminalName,
            [FromUri(Name = "version")] string terminalVersion)
        {
            //if either or both of the terminal name and version are not available, the action in question did not inform the correct URL to the external service
            if (string.IsNullOrEmpty(terminalName) || string.IsNullOrEmpty(terminalVersion))
            {
                EventManager.ReportUnparseableNotification(Request.RequestUri.AbsoluteUri, Request.Content.ReadAsStringAsync().Result);
            }

            //create a terminal event for event notification received
            EventManager.ReportExternalEventReceived(Request.Content.ReadAsStringAsync().Result);
            
             var result =await _event.RequestParsingFromTerminals(Request, terminalName, terminalVersion);


            //Check if responding to Salesforce
             if (terminalName == "terminalSalesforce")
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(result);
                return Content(HttpStatusCode.OK, doc, Configuration.Formatters.XmlFormatter);
            }
            
            
            return Ok();
        }

        // for tesing. This method will return result
        [HttpPost]
        [Route("eventsDebug")]
        public async Task<object> ProcessIncomingEventsDebug(
            [FromUri(Name = "dockyard_terminal")] string terminalName,
            [FromUri(Name = "version")] string terminalVersion)
        {
            //if either or both of the terminal name and version are not available, the action in question did not inform the correct URL to the external service
            if (string.IsNullOrEmpty(terminalName) || string.IsNullOrEmpty(terminalVersion))
            {
                EventManager.ReportUnparseableNotification(Request.RequestUri.AbsoluteUri, Request.Content.ReadAsStringAsync().Result);
            }

            //create a terminal event for event notification received
            EventManager.ReportExternalEventReceived(Request.Content.ReadAsStringAsync().Result);

            var result = await _event.RequestParsingFromTerminalsDebug(Request, terminalName, terminalVersion);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(result)
            };
        }
    }
}