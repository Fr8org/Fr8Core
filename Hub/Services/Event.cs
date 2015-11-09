using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using StructureMap;
using Data.Infrastructure;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;

namespace Hub.Services
{
    /// <summary>
    /// Event service implementation
    /// </summary>
    public class Event : IEvent
    {

        private readonly ITerminal _terminal;

        public Event()
        {
 
            _terminal = ObjectFactory.GetInstance<ITerminal>();
        }
        /// <see cref="IEvent.HandleTerminalIncident"/>
        public void HandleTerminalIncident(LoggingDataCm incident)
        {
            EventManager.ReportTerminalIncident(incident);
        }

        public void HandleTerminalEvent(LoggingDataCm eventDataCm)
        {
            EventManager.ReportTerminalEvent(eventDataCm);
        }

        public async Task<string> RequestParsingFromTerminals(HttpRequestMessage request, string pluginName, string pluginVersion)
        {
            //get required plugin URL by plugin name and its version
            string curPluginUrl = _terminal.ParseTerminalUrlFor(pluginName, pluginVersion, "events");


            //make POST with request content
            var result = await new HttpClient().PostAsync(new Uri(curPluginUrl, UriKind.Absolute), request.Content);

            //Salesforce response needs to be acknowledge
            if (pluginName == "terminalSalesforce")
            {
                string xmlResponse = result.Content.ReadAsAsync<string>().Result;
                return xmlResponse;
            }

            return "ok";
        }

        public async Task<string> RequestParsingFromTerminalsDebug(HttpRequestMessage request, string pluginName, string pluginVersion)
        {
            //get required plugin URL by plugin name and its version
            string curPluginUrl = _terminal.ParseTerminalUrlFor(pluginName, pluginVersion, "events");


            //make POST with request content
            var result = await new HttpClient().PostAsync(new Uri(curPluginUrl, UriKind.Absolute), request.Content);
            return  await result.Content.ReadAsStringAsync();
        }
    }
}

