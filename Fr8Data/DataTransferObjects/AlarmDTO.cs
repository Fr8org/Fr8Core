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

        //TODO: is this necessary
        [JsonProperty("terminalName")]
        public string TerminalName { get; set; }
        //TODO: is this necessary
        [JsonProperty("terminalVersion")]
        public string TerminalVersion { get; set; }
        //TODO: is this necessary
        [JsonProperty("actionDTO")]
        public ActivityDTO ActivityDTO { get; set; }
    }
}