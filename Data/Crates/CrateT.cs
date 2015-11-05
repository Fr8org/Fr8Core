using Data.Interfaces.Manifests;
using Newtonsoft.Json;

namespace Data.Crates
{
    [JsonConverter(typeof(DenySerializationConverter), "Crate can't be directly serialized to JSON. Convert it to CrateDTO.")]
    public class Crate<T> : Crate
    {
        public T Content
        {
            get { return Get<T>(); }
        }

        public Crate(Crate crate)
            : base(crate.ManifestType, crate.Id)
        {
            Label = crate.Label;
            KnownContent = crate.Get<T>();
        }

        public static Crate<T> FromContent(string label, T content)
        {
            return new Crate<T>(FromContentUnsafe(label, content));
        }
     
    }

}
