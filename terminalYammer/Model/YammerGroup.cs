using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace terminalYammer.Model
{
    public class YammerGroup
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("select_name")]
        public string SelectName { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("feed_description")]
        public string Description { get; set; }

        [JsonProperty("ordering_index")]
        public int OrderingIndex { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("id")]
        public string GroupID { get; set; }

        [JsonProperty("private")]
        public bool IsPrivate { get; set; }

    }
}