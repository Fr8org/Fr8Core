using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StructureMap;
using Data.Interfaces.DataTransferObjects;
using Utilities.Serializers.Json;

namespace Data.Crates.Helpers
{
    public class EventReportCrateFactory
    {
        public CrateDTO Create(String eventName, String palletId, params CrateDTO[] crates)
        {
            return Create(eventName, palletId, crates.ToList());
        }

        public CrateDTO Create(String eventName, String palletId, List<CrateDTO> crates)
        {
            var eventDTO = new EventDTO
            {
                EventName = eventName,
                PalletId = palletId,
                CrateStorage = crates
            };
            return Create(eventDTO);
        }

        public CrateDTO Create(EventDTO eventDTO)
        {
            var eventDTOContent = JsonConvert.SerializeObject(eventDTO);
            return new CrateDTO()
            {
                Id = Guid.NewGuid().ToString(),
                Label = "Dockyard Plugin Event or Incident Report",
                Contents = eventDTOContent,
                ManifestType = "Dockyard Plugin Event or Incident Report",
                ManifestId = 2
            };
        }
    }
}
