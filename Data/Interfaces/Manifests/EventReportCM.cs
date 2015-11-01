using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;
using Data.Constants;

namespace Data.Interfaces.Manifests
{
    public class EventReportCM : Manifest
    {
        public string EventNames { get; set; }
        public string ContainerDoId { get; set; }
        public string ExternalAccountId { get; set; }
        public List<CrateDTO> EventPayload { get; set; }

        public string Source { get; set; }

        public EventReportCM()
            : base(Constants.MT.StandardEventReport)
        {
            EventPayload = new List<CrateDTO>();
        }
    }
}
