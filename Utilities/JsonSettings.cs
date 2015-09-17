using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Utilities
{
    public static class JsonSettings
    {
        public static JsonSerializerSettings CamelCase = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
    }
}
