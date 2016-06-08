using Fr8.Infrastructure.Utilities.Serializers.Json;

namespace Fr8.Infrastructure.Utilities
{
    public class JsonPackager
    {
        private JsonSerializer jsonSerializer;

        public JsonPackager()
        {
            jsonSerializer = new JsonSerializer();
        }
        public string Pack(object dataObject)
        {
            return jsonSerializer.Serialize(dataObject);
        }
    }
}
