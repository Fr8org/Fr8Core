using Data.Entities;
using Data.States.Templates;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class TerminalDTO 
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("terminalStatus")]
        public int TerminalStatus { get; set; }
        public _TerminalStatusTemplate TerminalStatusTemplate { get; set; }
        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }
        [JsonProperty("subscriptionRequired")]
        public bool SubscriptionRequired { get; set; }
        public virtual Fr8AccountDO UserDO { get; set; }

    }
}