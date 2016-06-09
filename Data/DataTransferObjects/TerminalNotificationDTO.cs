using Newtonsoft.Json;
using System;

namespace Fr8Data.DataTransferObjects
{
    public class TerminalNotificationDTO
    {
        //TODO create type enum for notifications
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("terminalName")]
        public string TerminalName { get; set; }

        [JsonProperty("terminalVersion")]
        public string TerminalVersion { get; set; }

        [JsonProperty("activityName")]
        public string ActivityName { get; set; }

        [JsonProperty("activityVersion")]
        public string ActivityVersion { get; set; }
    }
}