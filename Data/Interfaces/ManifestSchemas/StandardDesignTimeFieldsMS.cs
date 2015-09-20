using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;

namespace Data.Interfaces.ManifestSchemas
{
    public class StandardDesignTimeFieldsMS
    {
        public StandardDesignTimeFieldsMS()
        {
            Fields = new List<FieldDTO>();
        }

        public List<FieldDTO> Fields { get; set; }
    }
}
