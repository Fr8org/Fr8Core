using AutoMapper;
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

            var serializer = new JsonSerializer();
            var curObject = serializer.Deserialize<T>(jsonString);

            return curObject;
        }
    }
}
