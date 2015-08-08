using Data.Entities;

namespace Core.Interfaces
{
    public interface IEventService
    {
        bool HandlePluginIncident(HistoryItemDO incident);
    }
}
