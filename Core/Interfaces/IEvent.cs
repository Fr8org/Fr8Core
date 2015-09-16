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
        void HandlePluginEvent(LoggingData eventData);

        void Process(string curExternalEventPayload);

        /// <summary>
        /// Gets the required plugin service URL by Plugin Name and its version
        /// </summary>
        /// <param name="curPluginName">Name of the required plugin</param>
        /// <param name="curPluginVersion">Version of the required plugin</param>
        /// <returns>End Point URL of the required plugin</returns>
        string GetPluginUrl(string curPluginName, string curPluginVersion);
    }
}
