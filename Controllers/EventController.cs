using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Web.Http;
using StructureMap;
using Data.Crates.Helpers;
using Data.Infrastructure;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Hub.Services;

namespace Web.Controllers
{
    /// <summary>
    /// Central logging Events controller
    /// </summary>
    public class EventController : ApiController
    {
        private readonly IEvent _event;
        private readonly ICrateManager _crate;
      

        private delegate void EventRouter(LoggingData loggingData);

        public EventController()
        {
            _event = ObjectFactory.GetInstance<IEvent>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
            
        }

        private EventRouter GetEventRouter(EventDTO eventDTO)
        {
            if (eventDTO.EventName.Equals("Plugin Incident"))
            {
                return _event.HandlePluginIncident;
            }

            if (eventDTO.EventName.Equals("Plugin Event"))
            {
                return _event.HandlePluginEvent;
            }

            throw new InvalidOperationException("Unknown EventDTO with name: " + eventDTO.EventName);
        }

        [HttpPost]
        public IHttpActionResult Post(CrateDTO submittedEventsCrate)
        {

            var eventDTO = _crate.GetContents<EventDTO>(submittedEventsCrate);

            //Request of alex to keep things simple for now
            if (eventDTO.CrateStorage.Count != 1)
            {
                throw new InvalidOperationException("Only single crate can be processed for now.");
            }

            EventRouter currentRouter = GetEventRouter(eventDTO);

            var errorMsgList = new List<string>();
            foreach (var crateDTO in eventDTO.CrateStorage)
            {
                if (crateDTO.ManifestType != "Dockyard Plugin Event or Incident Report")
                {
                    errorMsgList.Add("Don't know how to process an EventReport with the Contents: " + crateDTO.Contents);
                    continue;
                }
                var loggingData =_crate.GetContents<LoggingData>(crateDTO);
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
         * the action should query for the endpoint and add plugin name and its version with it.
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
            [FromUri(Name = "dockyard_plugin")] string pluginName,
            [FromUri(Name = "version")] string pluginVersion)
        {
            //if either or both of the plugin name and version are not available, the action in question did not inform the correct URL to the external service
            if (string.IsNullOrEmpty(pluginName) || string.IsNullOrEmpty(pluginVersion))
            {
                EventManager.ReportUnparseableNotification(Request.RequestUri.AbsoluteUri, Request.Content.ReadAsStringAsync().Result);
            }

            //create a plugin event for event notification received
            EventManager.ReportExternalEventReceived(Request.Content.ReadAsStringAsync().Result);
            
             var result =await _event.RequestParsingFromPlugins(Request, pluginName, pluginVersion);


            //Check if responding to Salesforce
            if(pluginName=="pluginSalesforce")
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
            [FromUri(Name = "dockyard_plugin")] string pluginName,
            [FromUri(Name = "version")] string pluginVersion)
        {
            //if either or both of the plugin name and version are not available, the action in question did not inform the correct URL to the external service
            if (string.IsNullOrEmpty(pluginName) || string.IsNullOrEmpty(pluginVersion))
            {
                EventManager.ReportUnparseableNotification(Request.RequestUri.AbsoluteUri, Request.Content.ReadAsStringAsync().Result);
            }

            //create a plugin event for event notification received
            EventManager.ReportExternalEventReceived(Request.Content.ReadAsStringAsync().Result);

            var result = await _event.RequestParsingFromPluginsDebug(Request, pluginName, pluginVersion);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(result)
            };
        }
    }
}