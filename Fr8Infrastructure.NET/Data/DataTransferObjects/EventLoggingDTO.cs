using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class EventLoggingDTO
    {
        [JsonProperty(PropertyName = "eventName")]
        public string EventName { get; set; }

        [JsonProperty(PropertyName = "customerId")]
        public string CustomerId { get; set; }

        [JsonProperty(PropertyName = "objectId")]
        public string ObjectId { get; set; }

        [JsonProperty(PropertyName = "data")]
        public string Data { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
    }
}
