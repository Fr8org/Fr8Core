using System.Collections.Generic;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Data.Manifests
{
    public class StandardQueryCM : Manifest
    {
        public StandardQueryCM()
			  :base(MT.StandardQueryCrate)
        {
            Queries = new List<QueryDTO>();
        }

        public StandardQueryCM(IEnumerable<QueryDTO> queries) : this()
        {
            Queries.AddRange(queries);
        }

        public StandardQueryCM(params QueryDTO[] queries) : this()
        {
            Queries.AddRange(queries);
        }


        public List<QueryDTO> Queries { get; set; }
    }
}
