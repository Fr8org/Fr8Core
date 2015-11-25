using System;
using Newtonsoft.Json.Linq;

namespace Data.Interfaces.DataTransferObjects
{   
    public class PayloadDTO
    {
        public PayloadDTO(Guid processId)
        {
            ProcessId = processId;
        }

        public CrateStorageDTO CrateStorage { get; set; }

        public Guid ProcessId { get; set; }
    }
}
