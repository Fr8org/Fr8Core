using System;

namespace Data.Crates
{
    [AttributeUsage(AttributeTargets.Field)]
    public class CrateManifestTypeDescriptionAttribute : Attribute
    {
        public readonly string TypeName;

        public CrateManifestTypeDescriptionAttribute(string typeName)
        {
            TypeName = typeName;
        }
    }
}