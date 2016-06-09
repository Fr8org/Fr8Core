using AutoMapper;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

            var serializer = new Fr8.Infrastructure.Utilities.Serializers.Json.JsonSerializer();
            serializer.Settings.MissingMemberHandling = MissingMemberHandling.Ignore;

            var curObject = serializer.Deserialize<T>(jsonString);

            return curObject;
        }
    }

    public class CrateStorageFromStringConverter : ITypeConverter<string, CrateStorageDTO>
    {
        public CrateStorageDTO Convert(ResolutionContext context)
        {
            var jsonString = context.SourceValue as string;
            return Convert(jsonString);
        }

        public static CrateStorageDTO Convert(string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return null;
            }

            var storageDto = JsonConvert.DeserializeObject<CrateStorageDTO>(jsonString);

            if (storageDto != null && storageDto.Crates != null)
            {
                foreach (var crateDto in storageDto.Crates)
                {
                    // looks like we found records id "old" format
                    var value = crateDto.Contents as JValue;
                    if (value != null && value.Value is string)
                    {
                        try
                        {
                            crateDto.Contents = JsonConvert.DeserializeObject<JToken>((string) value.Value);
                        }
                        catch
                        {
                            // do nothing. We can't deserializer contents. May be it is just a string?
                        }
                    }
                }
            }

            return storageDto;
        }
    }
}
