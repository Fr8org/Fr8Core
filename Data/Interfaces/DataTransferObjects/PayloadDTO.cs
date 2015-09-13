using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces.DataTransferObjects
{   
    public class PayloadDTO
    {
        public PayloadDTO(string crateStorage, int processId)
        {
            CrateStorage = crateStorage;
            ProcessId = processId;
        }

        public string CrateStorage{ get; set; }
        public int ProcessId { get; set; }
    }
}
