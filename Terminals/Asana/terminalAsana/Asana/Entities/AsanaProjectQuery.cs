using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace terminalAsana.Asana.Entities
{
    public class AsanaProjectQuery
    {
        [JsonProperty("archived")]
        public bool Archived { get; set; }

        [JsonProperty("team")]
        public string Team { get; set; }

        [JsonProperty("workspace")]
        public string Workspace { get; set; }
    }
}