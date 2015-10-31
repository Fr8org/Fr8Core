using Newtonsoft.Json.Linq;

namespace Data.Crates
{
    public interface IManifestSerializer
    {
        void Initialize(ICrateStorageSerializer storageSerializer);
        object Deserialize(JToken crateContent);
        JToken Serialize(object content);
    }
}