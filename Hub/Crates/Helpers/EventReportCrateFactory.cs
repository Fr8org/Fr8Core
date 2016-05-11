using System;
using System.Collections.Generic;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Hub.Managers;
using StructureMap;

namespace Hub.Crates.Helpers
{
    public class EventReportCrateFactory
    {
        public Crate Create(String eventName, String palletId, params Crate[] crates)
        {
            return Create(eventName, palletId, (IEnumerable<Crate>) crates);
        }
//
        public Crate Create(String eventName, String palletId, IEnumerable<Crate> crates)
        {
            var eventDTO = new EventCM
            {
                EventName = eventName,
                PalletId = palletId,
            };

            eventDTO.CrateStorage.AddRange(crates);

            return Crate.FromContent("Fr8 Terminal Event or Incident Report", eventDTO);
        }

        public Crate Create(EventCM eventCm)
        {
            return Crate.FromContent("Fr8 Terminal Event or Incident Report", eventCm);
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
