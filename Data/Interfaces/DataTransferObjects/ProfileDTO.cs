using System;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class ProfileDTO
    { 
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
