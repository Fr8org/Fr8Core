using System.Threading.Tasks;
using Data.Entities;
using System.Collections.Generic;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;

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
        void HandleTerminalIncident(LoggingDataCM incident);
        
        /// <summary>
        /// Handles Terminal Event 
        /// </summary>
        void HandleTerminalEvent(LoggingDataCM eventDataCm);

        Task ProcessInboundEvents(Crate curCrateStandardEventReport);
        Task LaunchProcess(PlanDO curPlan, Crate curEventData = null);
        Task LaunchProcesses(List<PlanDO> curPlans, Crate curEventReport);
    }
}
