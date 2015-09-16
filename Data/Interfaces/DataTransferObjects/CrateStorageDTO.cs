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
            CrateDTO = new List<CrateDTO>();
        }

        [JsonProperty("CrateStorageDTO")]
        public List<CrateDTO> CrateDTO { get; set; }
    }
}
