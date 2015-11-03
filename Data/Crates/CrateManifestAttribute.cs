using System;

namespace Data.Crates
{
    public class CrateManifestAttribute : Attribute
    {
        public readonly Enum ManifestType;
        public readonly Type Serializer;
        
        public CrateManifestAttribute(object manifestId, Type serializer = null)
        {
            Serializer = serializer;
            ManifestType = (Enum)manifestId;
        }

        public CrateManifestAttribute(Type serializer)
        {
            Serializer = serializer;
        }
    }
}