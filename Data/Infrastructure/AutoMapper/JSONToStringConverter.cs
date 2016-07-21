using AutoMapper;
using Newtonsoft.Json;

namespace Data.Infrastructure.AutoMapper
{
    public class JSONToStringConverter<T> : ITypeConverter<T, string>
        where T : class
    {
        public string Convert(ResolutionContext context)
        {
            var curObject = context.SourceValue as T;
            if (curObject == null)
            {
                return null;
            }

            var serializer = new Fr8.Infrastructure.Utilities.Serializers.Json.JsonSerializer();
            serializer.Settings.MissingMemberHandling = MissingMemberHandling.Ignore;

            var jsonStr = serializer.Serialize(curObject);

            return jsonStr;
        }
    }

    public class JsonToStringConverterNoMagic<T> : ITypeConverter<T, string>
       where T : class
    {
        public string Convert(ResolutionContext context)
        {
            var curObject = context.SourceValue as T;
            if (curObject == null)
            {
                return null;
            }

            return JsonConvert.SerializeObject(context.SourceValue, Formatting.Indented);
        }
    }
}
