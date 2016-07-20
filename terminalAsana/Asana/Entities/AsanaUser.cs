using System.Collections.Generic;
using Newtonsoft.Json;
using terminalAsana.Interfaces;

namespace terminalAsana.Asana.Entities
{

    public class AsanaUser 
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("photo")]
        public IDictionary<string,string> Photo{ get; set; }

        [JsonProperty("workspaces")]
        public IEnumerable<AsanaWorkspace> Workspaces { get; set; }
    }
}