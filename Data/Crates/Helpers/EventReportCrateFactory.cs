using System;
using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;

namespace Data.Crates.Helpers
{
    public class EventReportCrateFactory
    {
        public Crate Create(String eventName, String palletId, params Crate[] crates)
        {
            return Create(eventName, palletId, crates);
        }
//
        public Crate Create(String eventName, String palletId, IEnumerable<Crate> crates)
        {
            var eventDTO = new EventDTO
            {
                EventName = eventName,
                PalletId = palletId,
                CrateStorage = new CrateStorage(crates)
            };

            return Create(eventDTO);
        }

        public Crate Create(EventDTO eventDTO)
        {
            return Crate.FromContent("Dockyard Plugin Event or Incident Report", eventDTO);
//
//            var eventDTOContent = JsonConvert.SerializeObject(eventDTO);
//            return new CrateDTO()
//            {
//                Id = Guid.NewGuid().ToString(),
//                Label = "Dockyard Plugin Event or Incident Report",
//                Contents = eventDTOContent,
//                ManifestType = "Dockyard Plugin Event or Incident Report",
//                ManifestId = 2
//            };
        }
    }
}
