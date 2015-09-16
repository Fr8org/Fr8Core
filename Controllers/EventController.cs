using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Core.Interfaces;
using Core.Managers.APIManagers.Transmitters.Restful;
using Core.Services;
using Data.Crates.Helpers;
using Data.Interfaces.DataTransferObjects;
using StructureMap;

namespace Web.Controllers
{
    /// <summary>
    /// Central logging Events controller
    /// </summary>
    public class EventController : ApiController
    {
        private readonly IEvent _event;
        private readonly ICrate _crate;
        private readonly IPlugin _plugin;

        private delegate void EventRouter(LoggingData loggingData);

        public EventController()
        {
            _event = ObjectFactory.GetInstance<IEvent>();
            _crate = ObjectFactory.GetInstance<ICrate>();
            _plugin = ObjectFactory.GetInstance<IPlugin>();
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

        /// <summary>
        /// This HTTP Get allows acitons 
        /// </summary>
        [HttpGet]
        [Route("events/endpoint")]
        public string EventNotificationEndPoint()
        {
            //In development environment, Please uncomment this line.
            return "http://localhost:30643/events";

            //return "http://dockyard.company/events";
        }

        [HttpPost]
        [Route("events")]
        public async Task<string> Events(string dockyardPluginName, string dockyardPluginVersion)
        {
            //if either or both of the plugin name and version are not available, the action in question did not inform the correct URL to the external service
            if (string.IsNullOrEmpty(dockyardPluginName) || string.IsNullOrEmpty(dockyardPluginVersion))
            {
                _event.HandlePluginIncident(new LoggingData
                {
                    ObjectId = "EventController",
                    CustomerId = "not_applicable",
                    Data = "process_event_notificaiton_from_external_services",
                    PrimaryCategory = "Operations",
                    SecondaryCategory = "External Event Notifications",
                    Activity = "processing external service event notifications"
                });
            }

            //get required plugin URL by plugin name and its version
            string curPluginUrl = @"http://" + _plugin.GetPluginUrl(dockyardPluginName, dockyardPluginVersion);
            curPluginUrl += "/events";

            //make POST with request content
            string curResponseContent = await new HttpClient().PostAsync(new Uri(curPluginUrl, UriKind.Absolute), Request.Content).Result.Content.ReadAsStringAsync(); 

            //create a plugin event for event notification received
            _event.HandlePluginEvent(new LoggingData
            {
                ObjectId = "EventController",
                CustomerId = "not_applicable",
                Data = "process_notificaiton_from_external_services",
                PrimaryCategory = "EventNotificationReceived",
                SecondaryCategory = "Event Notification Received",
                Activity = string.Format("Processed event for {0}_v{1} on {2}.", dockyardPluginName, dockyardPluginVersion, curPluginUrl)
            });

            return curResponseContent;
        }
    }
}