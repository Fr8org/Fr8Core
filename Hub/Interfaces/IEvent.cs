using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;

namespace Hub.Interfaces
{
    /// <summary>
    /// Event service interface
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// Handles Terminal Incident
        /// </summary>
        void HandleTerminalIncident(LoggingDataCm incident);
        
        /// <summary>
        /// Handles Terminal Event 
        /// </summary>
        void HandleTerminalEvent(LoggingDataCm eventDataCm);
    }
}
