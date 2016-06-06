using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fr8Data.DataTransferObjects.RequestParameters
{
    public class PlansPostParams
    {
        public bool update_registrations { get; set; } = false;
        public string solution_name { get; set; }

    }
}
