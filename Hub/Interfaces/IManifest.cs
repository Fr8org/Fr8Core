using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hub.Interfaces
{
    public interface IManifest
    {
        CrateDTO GetById(int id);
        List<FieldDTO> convertPropertyToFields(PropertyInfo[] propertyInfo);
    }
}
