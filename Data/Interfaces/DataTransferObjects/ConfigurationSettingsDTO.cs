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
            Fields = new List<FieldDefinitionDTO>();
        }

        public List<FieldDefinitionDTO> Fields { get; set; }
    }
}
