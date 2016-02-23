using Data.Infrastructure.JsonNet;
using Data.States;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    [JsonConverter(typeof(FieldConverter))]
    public class FieldDTO
    {
        public string Key { get; set; }

        public string Value { get; set; }

        public string Tags { get; set; }

        public AvailabilityType Availability { get; set; }

        public Crates.CrateManifestType SourceCrateManifest { get; set; }

        public string SourceCrateLabel { get; set; }

        public FieldDTO()
        {
            //Availability = AvailabilityType.Configuration;
        }

        public FieldDTO(string key) : this()
        {
            Key = key;
        }
        public FieldDTO(string key, AvailabilityType availability) : this()
        {
            Key = key;
            Availability = availability;
        }
        public FieldDTO(string key, string value) : this()
        {
            Key = key;
            Value = value;
        }
        public FieldDTO(string key, string value, AvailabilityType availability) : this()
        {
            Key = key;
            Value = value;
            Availability = availability;
        }
    }
}
