using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class InternalDemoAccountDTO
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("hasDemoAccount")]
        public bool HasDemoAccount { get; set; }
    }
}
