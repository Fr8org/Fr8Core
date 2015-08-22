using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces.DataTransferObjects
{
    public class ConfigurationSettingsDTO
    {
        public ConfigurationSettingsDTO()
        {
            FieldDefinitions = new List<FieldDefinitionDTO>();
        }
        public List<FieldDefinitionDTO> FieldDefinitions { get; set; }
    }
}
