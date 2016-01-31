using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Infrastructure.JsonNet
{
    class TerminalActivityTemplateConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TerminalDTO);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var item = (TerminalDTO)value;
            writer.WriteStartObject();
            writer.WritePropertyName("name");
            writer.WriteValue(item.Name);
            writer.WritePropertyName("version");
            writer.WriteValue(item.Version);
            writer.WritePropertyName("endpoint");
            writer.WriteValue(item.Endpoint);
            writer.WriteEndObject();
            writer.Flush();
        }
    }
}
