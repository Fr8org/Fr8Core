using System;
using System.Collections.Generic;
using Fr8.Infrastructure.Data.Manifests;

namespace Fr8.Infrastructure.Data.Crates.Helpers
{
    public static class EventReportCrateFactory
    {
        public static Crate Create(String eventName, String palletId, params Crate[] crates)
        {
            return Create(eventName, palletId, (IEnumerable<Crate>) crates);
        }

        public static Crate Create(String eventName, String palletId, IEnumerable<Crate> crates)
        {
            var eventDTO = new EventReportCM
            {
                EventNames = eventName,
                ExternalAccountId = "system1@fr8.co"
            };

            eventDTO.EventPayload.AddRange(crates);

            return Crate.FromContent("Fr8 Terminal Fact or Incident Report", eventDTO);
        }

        public static Crate Create(EventReportCM eventCm)
        {
            return Crate.FromContent("Fr8 Terminal Fact or Incident Report", eventCm);
        }
    }
}
