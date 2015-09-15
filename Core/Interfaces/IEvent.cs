using Data.Interfaces.DataTransferObjects;

namespace Core.Interfaces
{
    /// <summary>
    /// Event service interface
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// Handles Plugin Incident
        /// </summary>
        void HandlePluginIncident(LoggingData incident);
        
        /// <summary>
        /// Handles Plugin Event 
        /// </summary>
        /// <param name="eventData"></param>
        void HandlePluginEvent(LoggingData eventData);
    }
}
