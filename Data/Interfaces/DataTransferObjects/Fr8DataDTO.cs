using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces.DataTransferObjects
{
    public class Fr8DataDTO
    {
        public ActivityDTO ActivityDTO { get; set; }
        public Guid? ContainerId { get; set; }

    }
}
