using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace terminalMailChimp.Models
{
    public class Member
    {
        [JsonProperty("email_address")]
        public string EmailAddress { get; set; }
        [JsonProperty("merge_fields")]
        public MergeFields MergeFields { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        public string ListId { get; set; }
    }

    public class MergeFields
    {
        [JsonProperty("FNAME")]
        public string FirstName { get; set; }
        [JsonProperty("LNAME")]
        public string LastName { get; set; }
    }
}