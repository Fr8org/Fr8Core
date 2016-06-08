using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Utilities;

namespace Fr8.Infrastructure.Data.Manifests
{
    [CrateManifestType(Int32.MinValue, null)]
    public abstract class Manifest
    {
        // here we cache primary key property list for each manifest type
        private static readonly Dictionary<Type, string[]> PkCache = new Dictionary<Type, string[]>();

        [JsonIgnore]
        [ManifestField(IsHidden = true)]
        public CrateManifestType ManifestType { get; }

        protected Manifest(MT manifestType)
            : this((int)manifestType, manifestType.GetEnumDisplayName())
        {
        }

        protected Manifest(int manifestType, string manifestName)
            : this(new CrateManifestType(manifestName, manifestType))
        {
        }

        protected Manifest(CrateManifestType manifestType)
        {
            ManifestType = manifestType;
        }

        // default implementation of GetPrimaryKey will return list of manifest properites marked with MtPrimaryKeyAttribute
        // developer can override this methods and return any property names he want
        public virtual string[] GetPrimaryKey()
        {
            return GetPrimaryKey(GetType());
        }

        private static string[] GetPrimaryKey(Type type)
        {
            lock (PkCache)
            {
                string[] pk;

                // try get list of PK properties from cache. If fails get this list using reflection
                if (!PkCache.TryGetValue(type, out pk))
                {
                    var pkList = new List<string>();

                    foreach (var prop in type.GetProperties())
                    {
                        if (prop.GetCustomAttribute(typeof(MtPrimaryKeyAttribute)) != null)
                        {
                            pkList.Add(prop.Name);
                        }
                    }

                    pk = pkList.ToArray();
                    PkCache[type] = pk;
                }

                return pk;
            }
        }
    }
}