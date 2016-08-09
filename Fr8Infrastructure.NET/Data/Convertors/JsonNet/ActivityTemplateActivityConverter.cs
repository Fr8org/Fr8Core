using System;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fr8.Infrastructure.Data.Convertors.JsonNet
{
    class ActivityTemplateActivityConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ActivityTemplateDTO);
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
            var customTerminalConverter = new TerminalActivityTemplateConverter();
            
            var item = (ActivityTemplateDTO)value;
            writer.WriteStartObject();
            writer.WritePropertyName("id");
            writer.WriteValue(item.Id.ToString());
            writer.WritePropertyName("name");
            writer.WriteValue(item.Name);
            writer.WritePropertyName("version");
            writer.WriteValue(item.Version);
            writer.WritePropertyName("label");
            writer.WriteValue(item.Label);
            writer.WritePropertyName("terminal");
            writer.WriteRawValue(JsonConvert.SerializeObject(item.Terminal, customTerminalConverter));
            writer.WritePropertyName("tags");
            writer.WriteValue(item.Tags);
            writer.WritePropertyName("type");
            writer.WriteValue(item.Type.ToString());
            writer.WritePropertyName("minPaneWidth");
            writer.WriteValue(item.MinPaneWidth);
            writer.WritePropertyName("needsAuthentication");
            writer.WriteValue(item.NeedsAuthentication);
            writer.WriteEndObject();
            writer.Flush();
        }
        
    }
}
