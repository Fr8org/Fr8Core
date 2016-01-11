using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using Data.Crates;
using Data.Entities;
using System.Collections.Generic;

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

        Task ProcessInboundEvents(Crate curCrateStandardEventReport);
        Task LaunchProcess(RouteDO curRoute, Crate curEventData = null);
        Task LaunchProcesses(List<RouteDO> curRoutes, Crate curEventReport);
    }
}
