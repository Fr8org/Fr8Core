using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace terminalAsana.Infrastructure
{

    public class AsanaUserInfo
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
        public IEnumerable<AsanaUserWorkspace> Workspaces { get; set; }
    }
}