
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Core.Interfaces;
using Core.Managers.APIManagers.Transmitters.Restful;
using Core.Utilities;
using Data.Entities.DocuSignParserModels;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using StructureMap;

namespace Core.Services
{
    /// <summary>
    /// Event service implementation
    /// </summary>
    public class Event : IEvent
    {
        /// <see cref="IEvent.HandlePluginIncident"/>
        public void HandlePluginIncident(LoggingData incident)
        {
            EventManager.ReportPluginIncident(incident);
        }

        public void HandlePluginEvent(LoggingData eventData)
        {
            EventManager.ReportPluginEvent(eventData);
        }
    }
}
