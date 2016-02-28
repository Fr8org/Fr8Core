using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Infrastructure.JsonNet
{
    class FieldListConverter : JsonConverter
    {
        private FieldConverter fieldConverter = new FieldConverter();
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<FieldDTO>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonArray = JArray.Load(reader);
            var instance = new List<FieldDTO>();

            foreach (JValue fieldJson in jsonArray)
            {
                var fieldDTO = JsonConvert.DeserializeObject<FieldDTO>(fieldJson.ToString(), fieldConverter);
                instance.Add(fieldDTO);
            }

            return instance;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var items = (List<FieldDTO>)value;
            writer.WriteStartArray();
            foreach (var item in items)
            {
                writer.WriteValue(JsonConvert.SerializeObject(item, fieldConverter));
            }
            writer.WriteEndArray();
            writer.Flush();
        }
    }
}
