using Fr8Data.States;
using Newtonsoft.Json;
using System;

namespace Fr8Data.Infrastructure.JsonNet
{
    class AvailabilityConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(AvailabilityType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var availabilityValue = reader.Value;
            AvailabilityType availability;
            if (availabilityValue == null)
            {
                availability = AvailabilityType.NotSet;
            }
            else
            {
                availability = (AvailabilityType) Enum.ToObject(typeof(AvailabilityType), availabilityValue);
            }
            return availability;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var item = (AvailabilityType)value;
            if (item == AvailabilityType.NotSet)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteValue(item);
            }
            writer.Flush();
        }
    }
}
