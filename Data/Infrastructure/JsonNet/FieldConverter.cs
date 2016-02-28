using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Data.Infrastructure.JsonNet
{
    class FieldConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(FieldDTO);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);

            JToken availabilityObj = null;
            var availabilityStr = "availability";
            bool hasAvailability = jsonObject.TryGetValue(availabilityStr, out availabilityObj);
            if (hasAvailability)
            {
                if (availabilityObj.Value<int?>() == null)
                {
                    AvailabilityType availability = AvailabilityType.NotSet;
                    JToken availabilityJToken = JToken.FromObject(availability);
                    jsonObject.Remove(availabilityStr);
                    jsonObject.Add(availabilityStr, availabilityJToken);
                }
            }

            var instance = (FieldDTO)Activator.CreateInstance(objectType);
            serializer.Populate(jsonObject.CreateReader(), instance);
            return instance;

        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var item = (FieldDTO)value;
            writer.WriteStartObject();
            writer.WritePropertyName("key");
            writer.WriteValue(item.Key);
            writer.WritePropertyName("value");
            writer.WriteValue(item.Value);
            writer.WritePropertyName("tags");
            writer.WriteValue(item.Tags);
            writer.WritePropertyName("availability");
            if (item.Availability == AvailabilityType.NotSet)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteValue(item.Availability);
            }
            
            if (item.SourceCrateManifest != null && item.SourceCrateManifest.Type != null)
            {
                writer.WritePropertyName("sourceCrateManifest");
                writer.WriteValue(JsonConvert.SerializeObject(item.SourceCrateManifest));
            }
            if (item.SourceCrateLabel != null)
            {
                writer.WritePropertyName("sourceCrateLabel");
                writer.WriteValue(item.SourceCrateLabel);
            }
            writer.WriteEndObject();
            writer.Flush();
        }
    }
}
