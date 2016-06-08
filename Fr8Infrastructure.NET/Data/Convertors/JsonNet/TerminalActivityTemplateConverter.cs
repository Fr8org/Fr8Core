using System;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fr8.Infrastructure.Data.Convertors.JsonNet
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
