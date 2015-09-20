using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;

namespace Data.Interfaces.ManifestSchemas
{
    public class StandardConfigurationControlsMS
    {
        public List<FieldDefinitionDTO> Controls { get; set; }

        public StandardConfigurationControlsMS()
        {
            Controls = new List<FieldDefinitionDTO>();
        }
    }


}
