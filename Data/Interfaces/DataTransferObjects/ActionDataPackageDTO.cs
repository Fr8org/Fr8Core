using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces.DataTransferObjects
{
    public class ActionDataPackageDTO
    {
        public ActionDataPackageDTO(ActivityDTO curActionDTO, PayloadDTO curPayloadDTO)
        {
            ActivityDTO = curActionDTO;
            PayloadDTO = curPayloadDTO;
        }
        public ActivityDTO ActivityDTO { get; set; }
        public PayloadDTO PayloadDTO { get; set; }
    }
}
