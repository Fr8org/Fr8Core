using AutoMapper;
using Newtonsoft.Json;
using Utilities.Serializers.Json;

namespace Data.Infrastructure.AutoMapper
{
    public class StringToJSONConverter<T> : ITypeConverter<string, T>
        where T : class
    {
        public T Convert(ResolutionContext context)
        {
            var jsonString = context.SourceValue as string;
            if (jsonString == null)
            {
                return null;
            }

            var serializer = new Utilities.Serializers.Json.JsonSerializer();
            serializer.Settings.MissingMemberHandling = MissingMemberHandling.Ignore;

            var curObject = serializer.Deserialize<T>(jsonString);

            return curObject;
        }
    }

    public class StringToJsonConverterNoMagic<T> : ITypeConverter<string, T>
        where T : class
    {
        public T Convert(ResolutionContext context)
        {
            var jsonString = context.SourceValue as string;
            if (jsonString == null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<T>(jsonString);
        }
    }
}
