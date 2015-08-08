using Data.Entities;

namespace Web.ViewModels
{
    public class EventDTO
    {
        public string Source { get; set; }

        public string EventType { get; set; }

        public HistoryItemDO Data { get; set; }
    }
}