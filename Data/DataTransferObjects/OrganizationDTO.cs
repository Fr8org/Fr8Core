using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fr8Data.DataTransferObjects
{
    public class OrganizationDTO
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("themeName")]
        public string ThemeName { get; set; }

        [JsonProperty("backgroundColor")]
        public string BackgroundColor { get; set; }

        [JsonProperty("logoUrl")]
        public string LogoUrl { get; set; }
    
    }
}
