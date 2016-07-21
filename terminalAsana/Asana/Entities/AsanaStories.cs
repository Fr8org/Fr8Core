using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using terminalAsana.Interfaces;

namespace terminalAsana.Asana.Entities
{
    public class AsanaStory
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("created_by")]
        public AsanaUser CreatedBy { get; set; }

        [JsonProperty("hearted")]
        public bool Hearted { get; set; }

        [JsonProperty("hearts")]
        public IEnumerable<AsanaUser> Hearts { get; set; }

        [JsonProperty("num_hearts")]
        public int NumHearts { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("html_text")]
        public string HtmlText { get; set; }
        
        [JsonProperty("target")]
        public AsanaTask Target { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

    }
}