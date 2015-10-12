using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;

namespace Data.Interfaces.ManifestSchemas
{
    public class EventReportCM
    {
        public string EventNames { get; set; }
        public string ProcessDOId { get; set; }
        public string ExternalAccountId { get; set; }
        public List<CrateDTO> EventPayload { get; set; }

        public string Source { get; set; }

        public EventReportCM()
        {
            EventPayload = new List<CrateDTO>();
        }
}
}
