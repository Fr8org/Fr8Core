using System.Collections.Generic;
using Data.Constants;
using Data.Interfaces.DataTransferObjects;

namespace Data.Interfaces.Manifests
{
    public class StandardDesignTimeFieldsCM : Manifest
    {
        public StandardDesignTimeFieldsCM()
			  : base(Constants.MT.StandardDesignTimeFields)
        {
            Fields = new List<FieldDTO>();
        }

        public StandardDesignTimeFieldsCM(IEnumerable<FieldDTO> fields) : this()
        {
            Fields.AddRange(fields);
        }

        public StandardDesignTimeFieldsCM(params FieldDTO[] fields) : this()
        {
            Fields.AddRange(fields);
        }

        public List<FieldDTO> Fields { get; set; }
    }
}
