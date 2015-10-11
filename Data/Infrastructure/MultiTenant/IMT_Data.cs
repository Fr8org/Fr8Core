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
    public interface IMT_Data
    {
        Data.Entities.MT_Data Create(BaseMultiTenantObject curMTO, Data.Entities.MT_Object correspondingMTObject);
    }
}
