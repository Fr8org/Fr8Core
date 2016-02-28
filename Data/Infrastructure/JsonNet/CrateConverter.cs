using Data.Interfaces.DataTransferObjects;
using Data.States;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Infrastructure.JsonNet
{
    class CrateConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(CrateDTO);
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

            var instance = (CrateDTO)Activator.CreateInstance(objectType);
            serializer.Populate(jsonObject.CreateReader(), instance);
            return instance;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var item = (CrateDTO)value;
            writer.WriteStartObject();
            writer.WritePropertyName("manifestType");
            writer.WriteValue(item.ManifestType);
            writer.WritePropertyName("manifestId");
            writer.WriteValue(item.ManifestId);
            writer.WritePropertyName("manufacturer");
            writer.WriteValue(item.Manufacturer);
            writer.WritePropertyName("manifestRegistrar");
            writer.WriteValue(item.ManifestRegistrar);
            writer.WritePropertyName("id");
            writer.WriteValue(item.Id);
            writer.WritePropertyName("label");
            writer.WriteValue(item.Label);
            writer.WritePropertyName("contents");
            item.Contents.WriteTo(writer);
            writer.WritePropertyName("parentCrateId");
            writer.WriteValue(item.ParentCrateId);
            writer.WritePropertyName("createTime");
            writer.WriteValue(item.CreateTime);
            writer.WritePropertyName("availability");
            if (item.Availability == AvailabilityType.NotSet)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteValue(item.Availability);
            }
            writer.WriteEndObject();
            writer.Flush();
        }
    }
}
