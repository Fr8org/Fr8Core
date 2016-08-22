using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace terminalYammer.Model
{
    public class YammerAccessToken
    {
        [JsonProperty("access_token")]
        public YammerTokenResponse TokenResponse { get; set; }

        public YammerAccessToken()
        {
            this.TokenResponse = new YammerTokenResponse();
        }
    }      
}