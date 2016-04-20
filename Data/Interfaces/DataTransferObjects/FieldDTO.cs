using Data.Crates;
using Data.Infrastructure.JsonNet;
using Data.States;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    [JsonConverter(typeof(FieldConverter))]
    [System.Diagnostics.DebuggerDisplay("Key = '{Key}', Value = '{Value}'")]
    public class FieldDTO : System.ICloneable
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("fieldType")]
        public string FieldType { get; set; }

        [JsonProperty("isRequired")]
        public bool IsRequired { get; set; }

        [JsonProperty("tags")]
        public string Tags { get; set; }

        [JsonProperty("availability")]
        public AvailabilityType Availability { get; set; }

        [JsonProperty("sourceCrateManifest")]
        public CrateManifestType SourceCrateManifest { get; set; }

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

        public FieldDTO Clone()
        {
            return new FieldDTO()
            {
                Key = this.Key,
                Value = this.Value,
                Tags = this.Tags,
                Availability = this.Availability,
                SourceCrateManifest = this.SourceCrateManifest,
                SourceCrateLabel = this.SourceCrateLabel
            };
        }

        object System.ICloneable.Clone()
        {
            return Clone();
        }
    }
}
