using System.Collections.Generic;
using Data.Constants;
using Data.Interfaces.DataTransferObjects;

namespace Data.Interfaces.Manifests
{
    public class FieldDescriptionsCM : Manifest
    {
        public FieldDescriptionsCM()
			  : base(Constants.MT.FieldDescription)
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
