using System;

namespace Data.Crates
{
    public class CrateManifestTypeAttribute : Attribute
    {
        public readonly Enum ManifestType;
        
        public CrateManifestTypeAttribute(object manifestId)
        {
            ManifestType = (Enum)manifestId;
        }
    }
    
    public class CrateManifestSerializerAttribute : Attribute
    {
        public readonly Type Serializer;
        
        public CrateManifestSerializerAttribute(Type serializer)
        {
            Serializer = serializer;
        }
    }
}