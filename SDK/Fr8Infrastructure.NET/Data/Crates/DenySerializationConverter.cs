using System;
using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.Crates
{
    public class DenySerializationConverter : JsonConverter
    {
        private readonly string _message;

        public DenySerializationConverter(string message)
        {
            _message = message;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new InvalidOperationException(_message);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new InvalidOperationException(_message);
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
}