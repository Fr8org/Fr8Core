using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;

namespace Data.Interfaces.ManifestSchemas
{


    public class ManifestSchema
    {
        
    }

    public class StandardConfigurationControlsMS : ManifestSchema
    {
        public List<FieldDefinitionDTO> Controls { get; set; }

        public StandardConfigurationControlsMS()
        {
            Controls = new List<FieldDefinitionDTO>();
        }
    }


}
