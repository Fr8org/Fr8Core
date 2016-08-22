using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace terminalAsana.Asana.Entities
{
    public class AsanaTaskQuery
    {
        [JsonProperty("project")]
        public string Project { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("assignee")]
        public string Assignee { get; set; }

        [JsonProperty("workspace")]
        public string Workspace { get; set; }

        [JsonProperty("completed_since")]
        public string CompletedSince { get; set; }

        [JsonProperty("modified_since")]
        public string ModifiedSince { get; set; }

    }
}