using System;
using Newtonsoft.Json;

namespace terminalAsana.Asana
{
    //it is not one-to-one binding to returned value, this is class for encapsulate meaningful values
    public class OAuthToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// The type of token, in our case: bearer
        /// </summary>
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        
        /// <summary>
        /// Seconds to expiration, usually 3600
        /// </summary>
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        // oAuth returns seconds till expiration, so we need do calculate absolute DataTime value
        public DateTime ExpirationDate { get; set; }


    }
}