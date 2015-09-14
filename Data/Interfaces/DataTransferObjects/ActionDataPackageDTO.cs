using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces.DataTransferObjects
{
    public class ActionDataPackageDTO
    {
        public ActionDataPackageDTO(ActionDTO curActionDTO, PayloadDTO curPayloadDTO)
        {
            ActionDTO = curActionDTO;
            PayloadDTO = curPayloadDTO;
        }
        public ActionDTO ActionDTO { get; set; }
        public PayloadDTO PayloadDTO { get; set; }
    }
}
