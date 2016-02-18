using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;

namespace Data.Interfaces.Manifests
{
    public class StandardQueryFieldsCM : Manifest
    {
        public StandardQueryFieldsCM()
			  : base(Constants.MT.StandardQueryFields)
        {
            Fields = new List<QueryFieldDTO>();
        }

        public StandardQueryFieldsCM(IEnumerable<QueryFieldDTO> fields) : this()
        {
            Fields.AddRange(fields);
        }

        public List<QueryFieldDTO> Fields { get; set; }
    }
}
