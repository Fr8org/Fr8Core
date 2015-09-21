using Core.Interfaces;
using Data.Infrastructure;
using Data.Interfaces.DataTransferObjects;

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
