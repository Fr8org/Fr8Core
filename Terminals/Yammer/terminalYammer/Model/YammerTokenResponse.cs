using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace terminalYammer.Model
{
    public class YammerTokenResponse
    {
        [JsonProperty("user_id")]
        public string UserID { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }
    }    
}