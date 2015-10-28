using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;

namespace Data.Interfaces.Manifests
{
    public class StandardQueryCM : Manifest
    {
        public List<QueryDTO> Queries { get; set; }

         public StandardQueryCM()
			  :base(Constants.MT.StandardQueryCrate)
        {
        }
    }
}
