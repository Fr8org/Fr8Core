using System;
using System.Collections.Generic;
using Fr8Data.Crates;
using Fr8Data.States;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fr8Data.DataTransferObjects
{
    [System.Diagnostics.DebuggerDisplay("Key = '{Key}', Value = '{Value}'")]
    public class FieldDTO : System.ICloneable
    {
        public const string Data_AllowableValues = "allowableValues";


        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("fieldType")]
        public string FieldType { get; set; }

        [JsonProperty("isRequired")]
        public bool IsRequired { get; set; }

        [JsonProperty("tags", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string Tags { get; set; }

        [JsonProperty("availability", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public AvailabilityType Availability { get; set; }

        [JsonProperty("sourceCrateManifest", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public CrateManifestType SourceCrateManifest { get; set; }

        [JsonProperty("sourceCrateLabel", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string SourceCrateLabel { get; set; }

        [JsonProperty("sourceActivityId", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string SourceActivityId { get; set; }

        [JsonProperty("data", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, JToken> Data { get; set; }
        
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
            return new FieldDTO
            {
                Key = Key,
                Value = Value,
                Tags = Tags,
                Label = Label,
                Data = Data == null ? null : new Dictionary<string, JToken>(Data),
                Availability = Availability,
                SourceCrateManifest = SourceCrateManifest,
                SourceCrateLabel = SourceCrateLabel,
                SourceActivityId = SourceActivityId
            };
        }

        object System.ICloneable.Clone()
        {
            return Clone();
        }

        public T GetData<T>(string key)
        {
            JToken value;

            if (Data == null || !Data.TryGetValue(key, out value))
            {
                return default(T);
            }

            return value.ToObject<T>();
        }

        public void SetData(string key, object value)
        {
            if (Data == null)
            {
                Data = new Dictionary<string, JToken>();
            }

            Data[key] = JToken.FromObject(value);
        }
    }
}
