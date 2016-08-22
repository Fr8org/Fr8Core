using System.Collections.Generic;
using System.Reflection;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Hub.Interfaces
{
    public interface IManifest
    {
        Crate GetById(int id);
        List<FieldDTO> ConvertPropertyToFields(PropertyInfo[] propertyInfo);
    }
}
