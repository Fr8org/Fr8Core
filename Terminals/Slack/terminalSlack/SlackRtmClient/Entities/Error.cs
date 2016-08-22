using Newtonsoft.Json;

namespace terminalSlack.RtmClient.Entities
{
    public class Error
    {
        public int Code { get; set; }
        [JsonProperty("msg")]
        public string Message { get; set; }
    }
}
