using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using terminalAsana.Interfaces;

namespace terminalAsana.Asana.Entities
{
    public class AsanaTask 
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("assignee")]
        public AsanaUser Assignee { get; set; }

        [JsonProperty("assignee_status")]
        public string AssigneeStatus { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("completed")]
        public bool Completed { get; set; }

        [JsonProperty("completed_at")]
        public DateTime? CompletedAt { get; set; }

        [JsonProperty("due_on")]
        public DateTime? DueOn { get; set; }

        [JsonProperty("due_at")]
        public DateTime? DueAt { get; set; }

        [JsonProperty("external")]
        public string External { get; set; }

        [JsonProperty("followers")]
        public IEnumerable<AsanaUser> Followers { get; set; }

        [JsonProperty("hearted")]
        public bool Hearted { get; set; }

        [JsonProperty("hearts")]
        public IEnumerable<AsanaUser> Hearts  { get; set; }
        
        [JsonProperty("modified_at")]
        public DateTime ModifiedAt { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("notes")]
        public string Notes { get; set; }

        [JsonProperty("num_hearts")]
        public int NumHearts { get; set; }

        [JsonProperty("projects")]
        public IEnumerable<AsanaProject> Projects { get; set; }

        [JsonProperty("parent")]
        public AsanaTask Parent { get; set; }

        [JsonProperty("workspace")]
        public AsanaWorkspace Workspace { get; set; }

        [JsonProperty("memberships")]
        public IEnumerable<AsanaMembership> Memberships { get; set; }

        [JsonProperty("tags")]
        public IEnumerable<AsanaTag> Tags { get; set; }
    }
}