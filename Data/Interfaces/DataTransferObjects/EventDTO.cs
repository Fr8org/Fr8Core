using Data.Constants;
using Data.Crates;
using Data.Interfaces.Manifests;
using Newtonsoft.Json.Linq;

namespace Data.Interfaces.DataTransferObjects
{
    public class EventDTO : Manifest
    {
        public string EventName { get; set; }

        public string PalletId { get; set; }

        public JToken CrateStorage { get; set; }

        public EventDTO() 
            : base(MT.EventOrIncidentReport)
        {
        }
    }

    public class LoggingData : Manifest
    {
        public string ObjectId { get; set; }

        public string CustomerId { get; set; }

        public string Data { get; set; }

        public string PrimaryCategory { get; set; }

        public string SecondaryCategory { get; set; }

        public string Activity { get; set; }

        public LoggingData()
            : base(MT.LoggingData)
        {
        }
    }
    
}
