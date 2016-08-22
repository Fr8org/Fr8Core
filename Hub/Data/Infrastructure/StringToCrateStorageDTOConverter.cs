using Fr8Data.DataTransferObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fr8Data.Infrastructure
{
    public class StringToCrateStorageDTOConverter
    { 
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
