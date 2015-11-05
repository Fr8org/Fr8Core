using Newtonsoft.Json.Linq;

namespace Data.Interfaces.DataTransferObjects
{   
    public class PayloadDTO
    {
        public PayloadDTO(int processId)
        {
            ProcessId = processId;
        }

        public CrateStorageDTO CrateStorage{ get; set; }

        public int ProcessId { get; set; }
    }
}
