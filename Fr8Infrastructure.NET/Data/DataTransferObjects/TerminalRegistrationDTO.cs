using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class TerminalRegistrationDTO
    {
        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }
    }
}
