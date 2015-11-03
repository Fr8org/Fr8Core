using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Crates;
using Newtonsoft.Json.Linq;

namespace Data.Interfaces.DataTransferObjects
{
    public class EventDTO
    {
        public string EventName { get; set; }

        public string PalletId { get; set; }

        public CrateStorage CrateStorage { get; set; }
    }

    public class LoggingData
    {
        public string ObjectId { get; set; }

        public string CustomerId { get; set; }

        public string Data { get; set; }

        public string PrimaryCategory { get; set; }

        public string SecondaryCategory { get; set; }

        public string Activity { get; set; }
    }
}
