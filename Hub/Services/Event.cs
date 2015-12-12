using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using StructureMap;
using Data.Infrastructure;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;
using System.Configuration;

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
    }
}

