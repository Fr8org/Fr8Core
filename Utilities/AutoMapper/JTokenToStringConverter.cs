using AutoMapper;
using Newtonsoft.Json.Linq;

namespace Utilities.AutoMapper
{
    /// <summary>
    /// AutoMapper converter to convert JObject to string JSON representation.
    /// </summary>
    public class JTokenToStringConverter : ITypeConverter<JToken, string>
    {
        public string Convert(ResolutionContext context)
        {
            return ((JToken)context.SourceValue).ToString();
        }
    }
}
