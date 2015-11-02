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

namespace Data.Infrastructure.MultiTenant
{
    public interface IMT_Data
    {
        Data.Entities.MT_Data Create(string curFr8AccountId, Manifest curManifest, Data.Entities.MT_Object correspondingMTObject);
    }
}
