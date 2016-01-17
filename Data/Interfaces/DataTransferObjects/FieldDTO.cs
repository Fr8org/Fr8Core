using Data.States;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class FieldDTO
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("tags")]
        public string Tags { get; set; }

        [JsonProperty("availability")]
        public AvailabilityType Availability { get; set; }

        [JsonProperty("sourceCrateManifest")]
        public Crates.CrateManifestType SourceCrateManifest { get; set; }

        [JsonProperty("sourceCrateLabel")]
        public string SourceCrateLabel { get; set; }

        public FieldDTO()
        {
            //Availability = AvailabilityType.Configuration;
        }

        public FieldDTO(string key) : this()
        {
            Key = key;
        }

        public FieldDTO(string key, string value) : this()
        {
            Key = key;
            Value = value;
        }
    }
}
