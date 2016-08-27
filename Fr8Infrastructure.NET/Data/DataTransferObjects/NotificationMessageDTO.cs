using Newtonsoft.Json;
using System;
using Fr8.Infrastructure.Data.Constants;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class NotificationMessageDTO
    {
        [JsonProperty("notificationType")]
        public NotificationType NotificationType { get; set; }

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

        [JsonProperty("collapsed")]
        public bool Collapsed { get; set; }
    }
}