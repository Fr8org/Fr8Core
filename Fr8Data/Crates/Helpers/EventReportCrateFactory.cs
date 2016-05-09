using System;
using System.Collections.Generic;
using Fr8Data.DataTransferObjects;

namespace Fr8Data.Crates.Helpers
{
    public class EventReportCrateFactory
    {
        public Crate Create(String eventName, String palletId, params Crate[] crates)
        {
            return Create(eventName, palletId, (IEnumerable<Crate>) crates);
        }

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
        }
    }
}
