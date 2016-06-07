using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fr8.Infrastructure.Data.DataTransferObjects;

namespace fr8.Infrastructure.Data.DataTransferObjects
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
