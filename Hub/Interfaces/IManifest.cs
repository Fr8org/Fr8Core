using System.Collections.Generic;
using System.Reflection;
using fr8.Infrastructure.Data.Crates;
using fr8.Infrastructure.Data.DataTransferObjects;

namespace Hub.Interfaces
{
    public interface IManifest
    {
        Crate GetById(int id);
        List<FieldDTO> ConvertPropertyToFields(PropertyInfo[] propertyInfo);
    }
}
