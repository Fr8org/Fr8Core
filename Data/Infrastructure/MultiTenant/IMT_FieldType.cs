using System;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using Data.Interfaces;
using Data.Entities;
using System.Collections.Generic;
using System.Reflection;

namespace Data.Infrastructure.MultiTenant
{
    interface IMT_FieldType
    {
        Data.Entities.MT_FieldType GetOrCreateMT_FieldType(IUnitOfWork _uow, Type type);
    }
}
