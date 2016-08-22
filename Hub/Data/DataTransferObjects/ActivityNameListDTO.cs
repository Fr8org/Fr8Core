using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fr8Data.DataTransferObjects
{
    public class ActivityNameListDTO
    {
        public ActivityNameListDTO()
        {
            ActivityNames = new List<ActivityNameDTO>();
        }
        public List<ActivityNameDTO> ActivityNames { get; set; }
    }
}
