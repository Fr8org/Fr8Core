using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Fr8.Infrastructure.Utilities
{
    public static class JsonSettings
    {
        public static JsonSerializerSettings CamelCase = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
    }
}
