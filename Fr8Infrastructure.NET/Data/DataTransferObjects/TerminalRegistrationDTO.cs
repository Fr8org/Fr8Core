using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class TerminalRegistrationDTO
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }

        [JsonProperty("devUrl")]
        public string DevUrl { get; set; }

        [JsonProperty("operationalState")]
        public int OperationalState{ get; set; }

        [JsonProperty("participationState")]
        public int ParticipationState { get; set; }
    }
}
