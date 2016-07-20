using Fr8Data.States;
using Newtonsoft.Json;

namespace Fr8Data.Crates
{
    [JsonConverter(typeof(DenySerializationConverter), "Crate can't be directly serialized to JSON. Convert it to CrateDTO.")]
    public class Crate<T> : Crate
    {
        public T Content => Get<T>();

        public Crate(Crate crate)
            : base(crate.ManifestType, crate.Id, crate.Availability)
        {
            Label = crate.Label;
            KnownContent = crate.Get<T>();
        }

        public static Crate<T> FromContent(string label, T content)
        {
            return FromContent(label, content, AvailabilityType.NotSet);
        }

        public static Crate<T> FromContent(string label, T content, AvailabilityType availability)
        {
            return new Crate<T>(FromContentUnsafe(label, content, availability));
        }     
    }
}
