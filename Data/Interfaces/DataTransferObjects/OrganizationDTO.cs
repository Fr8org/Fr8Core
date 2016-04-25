using System.Collections.Generic;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class OrganizationDTO
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("themeName")]
        public string ThemeName { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("logoUrl")]
        public string LogoUrl { get; set; }
    
    }
}
