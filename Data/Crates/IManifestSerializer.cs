using Newtonsoft.Json.Linq;

namespace Fr8Data.Crates
{
    /// <summary>
    /// Describes custom crate manifest serilizer
    /// </summary>
    public interface IManifestSerializer
    {
        /// <summary>
        ///  This will be called once per CrateStorageSerializer. Use this if you need to serialize/deserialize nested CrateStorage.
        /// </summary>
        /// <param name="storageSerializer"></param>
        void Initialize(ICrateStorageSerializer storageSerializer);
        /// <summary>
        /// Deserialize manifest from JToken
        /// </summary>
        /// <param name="crateContent"></param>
        /// <returns></returns>
        object Deserialize(JToken crateContent);
        /// <summary>
        /// Serialize manifest to JToken
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        JToken Serialize(object content);
    }
}