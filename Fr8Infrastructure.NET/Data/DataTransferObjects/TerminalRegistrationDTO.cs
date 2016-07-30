using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class TerminalRegistrationDTO
    {
        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }

        [JsonProperty("devUrl")]
        public string DevUrl { get; set; }

        [JsonProperty("prodUrl")]
        public string ProdUrl { get; set; }
    }
}
