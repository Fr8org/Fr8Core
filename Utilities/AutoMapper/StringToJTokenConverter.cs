using AutoMapper;
using Newtonsoft.Json.Linq;

namespace Utilities.AutoMapper
{
    /// <summary>
    /// AutoMapper converter to convert string JSON representation to JObject.
    /// </summary>
    public class StringToJTokenConverter : ITypeConverter<string, JToken>
    {
        public JToken Convert(ResolutionContext context)
        {
            return JToken.Parse((string)context.SourceValue);
        }
    }
}
