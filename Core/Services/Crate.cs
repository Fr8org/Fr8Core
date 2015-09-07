using Core.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public class Crate : ICrate
    {
        public CrateDTO Create(string crateStorage)
        {
            List<CrateDTO> crates = JsonConvert.DeserializeObject<List<CrateDTO>>(crateStorage); //get the lists of crate inside CrateStorage JSON
            if (crates != null && crates.Count > 0)
            {
                CrateDTO crateDTO = crates[0];
                crateDTO.Id = Guid.NewGuid().ToString();
                return crateDTO;
            }
            else
            {
                throw new ArgumentNullException("Cannot deserialize Empty CrateStorage to Crate DTO");
            }
        }
    }
}
