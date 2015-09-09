using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class CrateStorageDTO
    {
        public CrateStorageDTO()
        {
            CratesDTO = new List<CrateDTO>();
        }

        [JsonProperty("CratesDTO")]
        public List<CrateDTO> CratesDTO { get; set; }
    }
}
