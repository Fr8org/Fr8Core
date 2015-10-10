using System;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using Data.Interfaces;
using Data.Entities;
using Data.Interfaces.MultiTenantObjects;
using System.Collections.Generic;
using System.Reflection;

namespace Data.Infrastructure.MultiTenant
{
    interface IMT_Object
    {
        Data.Entities.MT_Object GetOrCreateMT_Object(IUnitOfWork _uow, BaseMultiTenantObject curMTO, Type curDataType, Dictionary<Type, Data.Entities.MT_FieldType> typesDict);
    }
}
