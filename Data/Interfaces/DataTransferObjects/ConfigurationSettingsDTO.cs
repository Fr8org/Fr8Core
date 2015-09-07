using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class ConfigurationSettingsDTO
    {
        public ConfigurationSettingsDTO()
        {
            Fields = new List<FieldDefinitionDTO>();
            DataFields = new List<string>();
        }

        [JsonProperty("fields")]
        public List<FieldDefinitionDTO> Fields { get; set; }

        [JsonProperty("data-fields")]
        public List<string> DataFields { get; set; }
    }
}
