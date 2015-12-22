using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace terminalYammer.DataTransferObjects
{
    public class YammerAccessTokenDTO
    {
        [JsonProperty("access_token")]
        public YammerTokenResponseDTO TokenResponse { get; set; }

        public YammerAccessTokenDTO()
        {
            this.TokenResponse = new YammerTokenResponseDTO();
        }
    }      
}