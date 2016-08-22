using System;

namespace Data.Repositories.MultiTenant
{
    public interface IMtObjectConverter
    {
        MtObject ConvertToMt(object instance, MtTypeDefinition mtTypeDefinition);
        object ConvertToObject(MtObject mtObject);
        bool IsPrimitiveType(Type type);
        object ConvertFromDbCanonicalFormat(string value, MtTypeDefinition type);
        string ConvertToDbCanonicalFormat(object value);
    }
}
