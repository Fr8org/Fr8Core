using System;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using Data.Interfaces;
using Data.Entities;
using System.Collections.Generic;
using System.Reflection;
using Data.Interfaces.ManifestSchemas;

namespace Data.Infrastructure.MultiTenant
{
    interface IMT_Object
    {
        Data.Entities.MT_Object GetOrCreateMT_Object(IUnitOfWork _uow, Manifest curManifest, Type curDataType);
    }
}
