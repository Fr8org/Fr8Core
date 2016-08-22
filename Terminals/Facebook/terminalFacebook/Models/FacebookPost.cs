using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace terminalFacebook.Models
{
    public class FacebookPost
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("created_time")]
        public string created_time { get; set; }

        [JsonProperty("message")]
        public string message { get; set; }

        [JsonProperty("story")]
        public string story { get; set; }

        public string description { get; set; }

        public string from { get; set; }

        public string picture { get; set; }

        public string type { get; set; }
    }

    public class FacebookPaging
    {
        [JsonProperty("previous")]
        public string previous { get; set; }

        [JsonProperty("next")]
        public string next { get; set; }
    }

    public class GraphApiPostReply
    {
        [JsonProperty("data")]
        public List<FacebookPost> data { get; set; }

        [JsonProperty("paging")]
        public FacebookPaging paging { get; set; }
    }
}