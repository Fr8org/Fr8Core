using AutoMapper;
using Newtonsoft.Json.Linq;

namespace Utilities.AutoMapper
{
    /// <summary>
    /// AutoMapper converter to convert JObject to string JSON representation.
    /// </summary>
    public class JObjectToStringConverter : ITypeConverter<JObject, string>
    {
        public string Convert(ResolutionContext context)
        {
            return ((JObject)context.SourceValue).ToString();
        }
    }
}
