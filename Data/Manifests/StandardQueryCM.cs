using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8Data.DataTransferObjects;

namespace Fr8Data.Manifests
{
    public class StandardQueryCM : Manifest
    {
        public StandardQueryCM()
			  :base(Constants.MT.StandardQueryCrate)
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
