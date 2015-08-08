using Data.Entities;

namespace Core.Interfaces
{
    /// <summary>
    /// Event service interface
    /// </summary>
    public interface IEventService
    {
        /// <summary>
        /// Handles Plugin Incident
        /// </summary>
        bool HandlePluginIncident(HistoryItemDO incident);
    }
}
