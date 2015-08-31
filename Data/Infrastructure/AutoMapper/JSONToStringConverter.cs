using Data.Interfaces.DataTransferObjects;
using AutoMapper;
using Utilities.Serializers.Json;

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

            var serializer = new JsonSerializer();
            var jsonStr = serializer.Serialize(curObject);

            return jsonStr;
        }
    }
}
