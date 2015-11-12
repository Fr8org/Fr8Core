using System;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using Data.Interfaces;
using Data.Entities;
using System.Collections.Generic;
using System.Reflection;
using Data.Interfaces.Manifests;
using StructureMap;
using Data.Infrastructure.StructureMap;

namespace Data.Infrastructure.MultiTenant
{
    public class MT_Data : IMT_Data
    {
        private MT_Field _mtField;

        public MT_Data()
        {
            this._mtField = new MT_Field();
        }

        public Data.Entities.MT_Data Create(string curFr8AccountId, Manifest curManifest, Data.Entities.MT_Object correspondingMTObject)
        {
            var data = new Data.Entities.MT_Data();
            data.fr8AccountId = curFr8AccountId;
            data.CreatedAt = DateTime.UtcNow;
            data.UpdatedAt = DateTime.UtcNow;
            data.MT_Object = correspondingMTObject;
            return data;
        }
    }
}
