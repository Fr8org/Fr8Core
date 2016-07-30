using System.CodeDom;
using Newtonsoft.Json;

namespace terminalStatX.DataTransferObjects
{
    public class StatXAuthDTO 
    {
        [JsonProperty("apiKey")]
        public string ApiKey { get; set; }
        [JsonProperty("authToken")]
        public string AuthToken { get; set; }
    }
}