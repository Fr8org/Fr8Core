using Fr8Data.DataTransferObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Fr8Data.Infrastructure.JsonNet
{
    class TerminalActivityTemplateConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TerminalDTO);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var instance = (ActivityTemplateDTO)Activator.CreateInstance(objectType);
            serializer.Populate(jsonObject.CreateReader(), instance);
            return instance;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var item = (TerminalDTO)value;
            writer.WriteStartObject();
            writer.WritePropertyName("name");
            writer.WriteValue(item.Name);
            writer.WritePropertyName("label");
            writer.WriteValue(item.Label);
            writer.WritePropertyName("version");
            writer.WriteValue(item.Version);
            writer.WritePropertyName("endpoint");
            writer.WriteValue(item.Endpoint);
            writer.WriteEndObject();
            writer.Flush();
        }
    }
}
