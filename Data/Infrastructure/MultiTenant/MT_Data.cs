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
    public class MT_Data : IMT_Data
    {
        private MT_Field _mtField;

        public MT_Data()
        {
            this._mtField = new MT_Field();
        }

        public Data.Entities.MT_Data Create(BaseMultiTenantObject curMTO, Data.Entities.MT_Object correspondingMTObject)
        {
            var data = new Data.Entities.MT_Data();
            data.Name = curMTO.Name;
            data.CreatedAt = DateTime.Now;
            data.UpdatedAt = DateTime.Now;
            data.MT_Object = correspondingMTObject;
            return data;
        }
    }
}
