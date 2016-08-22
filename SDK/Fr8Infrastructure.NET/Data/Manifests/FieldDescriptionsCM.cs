using System.Collections.Generic;
using System.Linq;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Data.Manifests
{
    public class FieldDescriptionsCM : Manifest
    {
        public FieldDescriptionsCM()
			  : base(MT.FieldDescription)
        {
            Fields = new List<FieldDTO>();
        }

        public FieldDescriptionsCM(IEnumerable<FieldDTO> fields) : this()
        {
            Fields.AddRange(fields);
        }

        public FieldDescriptionsCM(params FieldDTO[] fields) : this()
        {
            Fields.AddRange(fields);
        }

        public List<FieldDTO> Fields { get; set; }
  
    }
    
}
