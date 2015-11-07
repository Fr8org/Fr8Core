using System;

namespace Data.Crates
{
    /// <summary>
    /// Use this attribute to specife crate manifest type. This is the alternative to deriving crate's manifest from Manifest base class.
    /// </summary>
    public class CrateManifestTypeAttribute : Attribute
    {
        public readonly Enum ManifestType;
        
        public CrateManifestTypeAttribute(object manifestId)
        {
            ManifestType = (Enum)manifestId;
        }
    }
    
    /// <summary>
    /// Use this attribute to specify custom serializer for crate manifest.
    /// </summary>
    public class CrateManifestSerializerAttribute : Attribute
    {
        public readonly Type Serializer;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializer">Serializer type. Must implement IManifestSerializer</param>
        public CrateManifestSerializerAttribute(Type serializer)
        {
            Serializer = serializer; 
        }
    }
}