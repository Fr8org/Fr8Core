using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Data.Crates;

namespace Hub.Interfaces
{
    public interface IManifest
    {
        Crate GetById(int id);
        List<FieldDTO> ConvertPropertyToFields(PropertyInfo[] propertyInfo);
    }
}
