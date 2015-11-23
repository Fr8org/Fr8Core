using Newtonsoft.Json;
using System;

namespace Data.Interfaces.DataTransferObjects
{
    public class AlarmDTO
    {
        [JsonProperty("startTime")]
        public string StartTime { get; set; }

        [JsonProperty("containerId")]
        public Guid ContainerId { get; set; }

        [JsonProperty("terminalName")]
        public string TerminalName { get; set; }

        [JsonProperty("terminalVersion")]
        public string TerminalVersion { get; set; }
    }
}
