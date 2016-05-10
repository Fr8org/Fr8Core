using System.Collections.Generic;
using System.Reflection;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;

namespace Hub.Interfaces
{
    public interface IManifest
    {
        Crate GetById(int id);
        List<FieldDTO> ConvertPropertyToFields(PropertyInfo[] propertyInfo);
    }
}
