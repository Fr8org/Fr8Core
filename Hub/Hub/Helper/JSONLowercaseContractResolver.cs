using Newtonsoft.Json.Serialization;

namespace Hub.Helper
{
    public class JSONLowercaseContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToLower();
        }
    }
}
