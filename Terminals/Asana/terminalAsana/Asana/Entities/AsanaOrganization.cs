using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using terminalAsana.Interfaces;

namespace terminalAsana.Asana.Entities
{
    public class AsanaOrganization
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}