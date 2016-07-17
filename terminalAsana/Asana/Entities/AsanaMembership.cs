using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace terminalAsana.Asana.Entities
{
    public class AsanaMembership
    {
        [JsonProperty("project")]
        public AsanaProject Project { get; set; }

        [JsonProperty("section")]
        public AsanaSection Section { get; set; }
    }
}