using Newtonsoft.Json;
using System;

namespace Fr8Data.DataTransferObjects
{
    public class AlarmDTO
    {
        [JsonProperty("startTime")]
        public DateTimeOffset StartTime { get; set; }

        [JsonProperty("containerId")]
        public Guid ContainerId { get; set; }
    }
}