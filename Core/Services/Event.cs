using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Core.Interfaces;
using Data.Infrastructure;
using Data.Interfaces.DataTransferObjects;
using StructureMap;

namespace Core.Services
{
    /// <summary>
    /// Event service implementation
    /// </summary>
    public class Event : IEvent
    {

        private readonly IPlugin _plugin;

        public Event()
        {
 
            _plugin = ObjectFactory.GetInstance<IPlugin>();
        }
        /// <see cref="IEvent.HandlePluginIncident"/>
        public void HandlePluginIncident(LoggingData incident)
        {
            EventManager.ReportPluginIncident(incident);
        }

        public void HandlePluginEvent(LoggingData eventData)
        {
            EventManager.ReportPluginEvent(eventData);
        }

        public async Task<string> RequestParsingFromPlugins(HttpRequestMessage request, string pluginName, string pluginVersion)

        {
            //get required plugin URL by plugin name and its version
            string curPluginUrl = _plugin.ParsePluginUrlFor(pluginName, pluginVersion, "events");


            //make POST with request content
            await new HttpClient().PostAsync(new Uri(curPluginUrl, UriKind.Absolute), request.Content);

            return "ok";
        }
    }
}

