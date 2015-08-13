using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces.DataTransferObjects
{
    public class EventDTO
    {
        public string Source { get; set; }

        public string EventType { get; set; }

        public EventData Data { get; set; }
    }

    public class EventData
    {
        public string ObjectId { get; set; }

        public string CustomerId { get; set; }

        public string Data { get; set; }

        public string PrimaryCategory { get; set; }

        public string SecondaryCategory { get; set; }

        public string Activity { get; set; }
    }
}
