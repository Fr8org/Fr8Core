using Fr8Data.DataTransferObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Fr8Data.Infrastructure.JsonNet
{
    public class WebServiceConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(WebServiceDTO);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var instance = (WebServiceDTO)Activator.CreateInstance(objectType);
            serializer.Populate(jsonObject.CreateReader(), instance);
            return instance;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value != null)
            {
                var item = (WebServiceDTO)value;
                writer.WriteStartObject();
                writer.WritePropertyName("id");
                writer.WriteValue(item.Id);
                writer.WritePropertyName("name");
                writer.WriteValue(item.Name);
                writer.WritePropertyName("iconPath");
                writer.WriteValue(item.IconPath);
                writer.WriteEndObject();
                writer.Flush();
            }
        }
    }
}
