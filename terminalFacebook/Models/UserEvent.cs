using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace terminalFacebook.Models
{
    public class UserNotification
    {
        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("entry")]
        public Entry[] Entry { get; set; }
    }

    public class Entry
    {
        [JsonProperty("time")]
        public string Time { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("changed_fields")]
        public string[] ChangedFields { get; set; }

        [JsonProperty("uid")]
        public string Uid { get; set; }

    }
}