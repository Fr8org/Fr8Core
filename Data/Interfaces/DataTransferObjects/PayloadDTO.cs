using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Data.Interfaces.DataTransferObjects
{   
    public class PayloadDTO
    {
        public PayloadDTO(int processId)
        {
            //CrateStorage = crateStorage;
            ProcessId = processId;
        }

        public JToken CrateStorage{ get; set; }
        public int ProcessId { get; set; }

//        public CrateStorageDTO CrateStorageDTO()
//        {
//            return JsonConvert.DeserializeObject<CrateStorageDTO>(this.CrateStorage);
//        }
//
//        public void UpdateCrateStorageDTO(List<CrateDTO> curCratesDTO)
//        {
//            CrateStorageDTO crateStorageDTO = new CrateStorageDTO();
//
//            if (!String.IsNullOrEmpty(CrateStorage))//if crateStorage is not empty deserialize it
//                crateStorageDTO = CrateStorageDTO();
//
//            crateStorageDTO.CrateDTO.AddRange(curCratesDTO);
//
//            this.CrateStorage = JsonConvert.SerializeObject(crateStorageDTO);
//        }

    }
}
