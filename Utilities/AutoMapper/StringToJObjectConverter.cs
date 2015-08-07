using AutoMapper;
using Newtonsoft.Json.Linq;

namespace Utilities.AutoMapper
{
    /// <summary>
    /// AutoMapper converter to convert string JSON representation to JObject.
    /// </summary>
    public class StringToJObjectConverter : ITypeConverter<string, JObject>
    {
        public JObject Convert(ResolutionContext context)
        {
            return JObject.Parse((string)context.SourceValue);
        }
    }
}
