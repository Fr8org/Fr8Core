using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fr8Data.DataTransferObjects.RequestParameters
{
    public class PlansGetParams
    {
        public Guid? id { get; set; }
        public Guid? activity_id { get; set; }
        public bool include_children { get; set; } = false;
        public string name { get; set; }
    }
}
