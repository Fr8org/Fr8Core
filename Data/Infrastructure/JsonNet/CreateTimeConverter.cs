using Newtonsoft.Json;
using System;

namespace Fr8Data.Infrastructure.JsonNet
{
    class CreateTimeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var createTime = reader.Value;
            if (createTime.ToString() == string.Empty)
            {
                createTime = default(DateTime);
            }
            else
            {
                createTime = (DateTime)createTime; 
            }
            return createTime;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var createTime = (DateTime)value;
            if (DateTime.Equals(createTime, default(DateTime)))
            {
                writer.WriteValue(string.Empty);
            }
            else
            {
                writer.WriteValue(createTime);
            }
            writer.Flush();
        }
    }
}
