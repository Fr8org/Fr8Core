using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using terminalAsana.Interfaces;

namespace terminalAsana.Asana.Entities
{
    public class AsanaProject
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("owner")]
        public AsanaUser Owner { get; set; }

        [JsonProperty("current_status")]
        public AsanaProjectStatus CurrentStatus { get; set; }

        [JsonProperty("due_date")]
        public DateTime DueDate { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("modified_at")]
        public DateTime ModifiedAt { get; set; }

        [JsonProperty("archived")]
        public bool Archived { get; set; }

        [JsonProperty("public")]
        public bool Public { get; set; }

        [JsonProperty("members")]
        public IEnumerable<AsanaUser> Members { get; set; }

        [JsonProperty("followers")]
        public IEnumerable<AsanaUser> Followers { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("notes")]
        public string Notes { get; set; }

        [JsonProperty("workspace")]
        public AsanaWorkspace Workspace { get; set; }

        [JsonProperty("team")]
        public AsanaOrganization Team { get; set; }
    }
}