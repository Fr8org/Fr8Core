using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Data.States;
using Data.Infrastructure.JsonNet;

namespace Data.Interfaces.DataTransferObjects
{
    [JsonConverter(typeof(CrateConverter))]
    public class CrateDTO
    {
        public string Id { get; set; }

        public string Label { get; set; }

        public JToken Contents { get; set; }

        public string ParentCrateId { get; set; }

        public string ManifestType { get; set; }

        public int ManifestId { get; set; }

        public ManufacturerDTO Manufacturer { get; set; }

        public DateTime CreateTime { get; set; }

        public AvailabilityType Availability { get; set; }
    }
}
