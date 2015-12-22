using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace terminalYammer.DataTransferObjects
{
    public class YammerTokenResponseDTO
    {
        [JsonProperty("user_id")]
        public string UserID { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }
    }    
}