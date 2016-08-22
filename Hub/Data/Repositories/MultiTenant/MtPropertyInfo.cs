namespace Data.Repositories.MultiTenant
{
    public class MtPropertyInfo
    {
        public readonly string Name;
        public readonly MtTypeDefinition DeclaringType;
        public readonly MtTypeDefinition MtPropertyType;
        public readonly int Index;

        public MtPropertyInfo(int index, string name, MtTypeDefinition declaringType, MtTypeDefinition mtPropertyType)
        {
            Index = index;
            Name = name;
            DeclaringType = declaringType;
            MtPropertyType = mtPropertyType;

        }
    }
}