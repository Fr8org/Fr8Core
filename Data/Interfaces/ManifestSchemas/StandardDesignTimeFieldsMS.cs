using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;

namespace Data.Interfaces.ManifestSchemas
{
    public class StandardDesignTimeFieldsMS
    {
        public List<FieldDTO> Fields { get; set; }

        public StandardDesignTimeFieldsMS()
        {
            Fields = new List<FieldDTO>();
        }

      
    }
}
