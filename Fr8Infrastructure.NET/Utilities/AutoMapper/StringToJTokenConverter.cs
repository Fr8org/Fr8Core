using AutoMapper;
using Newtonsoft.Json.Linq;

namespace Fr8.Infrastructure.Utilities.AutoMapper
{
    /// <summary>
    /// AutoMapper converter to convert string JSON representation to JObject.
    /// </summary>
    public class StringToJTokenConverter : ITypeConverter<string, JToken>
    {
        public JToken Convert(ResolutionContext context)
        {
            if (context.SourceValue == null) { return null; }

            return JToken.Parse((string)context.SourceValue);
        }
    }
}
