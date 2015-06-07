using Utilities.Serializers.Json;

namespace Utilities
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
