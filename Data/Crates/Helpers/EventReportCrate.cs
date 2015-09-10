using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;
using StructureMap;
using Utilities.Serializers.Json;

namespace Data.Crates.Helpers
{
    public static class EventReportCrate
    {
        public static CrateDTO Create(String eventName, String palletId, params CrateDTO[] crates)
        {
            return Create(eventName, palletId, crates.ToList());
        }

        public static CrateDTO Create(String eventName, String palletId, List<CrateDTO> crates)
        {
            var eventDTO = new EventDTO
            {
                EventName = eventName,
                PalletId = palletId,
                CrateStorage = crates
            };
            return Create(eventDTO);
        }

        public static CrateDTO Create(EventDTO eventDTO)
        {
            var serializer = new JsonSerializer();
            var eventDTOContent = serializer.Serialize(eventDTO);
            return new CrateDTO()
            {
                Id = Guid.NewGuid().ToString(),
                Label = "Dockyard Plugin Event or Incident Report",
                Contents = eventDTOContent,
                ManifestType = "Dockyard Plugin Event or Incident Report",
                ManifestId = 3
            };
        }
    }
}
