using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Manifests;

namespace Fr8.Infrastructure.Data.Crates
{
    internal static class ManifestTypeCache
    {
        /**********************************************************************************/
        // Declarations
        /**********************************************************************************/

        private static readonly Dictionary<Type, CrateManifestType> ManifestsCache = new Dictionary<Type, CrateManifestType>();
        private static readonly object GlobalTypeCacheLock = new object();

        private static readonly Dictionary<string, int> _nameMap;

        static ManifestTypeCache()
        {
            _nameMap = new Dictionary<string, int>();

            var mtEnumType = typeof(MT);

            var names = Enum.GetNames(mtEnumType);
            foreach (var name in names)
            {
                var members = mtEnumType.GetMember(name);
                if (members == null || members.Length == 0)
                {
                    continue;
                }

                var attribute = members[0].GetCustomAttribute(typeof(DisplayAttribute), false);
                if (attribute == null)
                {
                    continue;
                }

                var mtName = ((DisplayAttribute)attribute).Name;
                _nameMap.Add(mtName, (int)((MT)Enum.Parse(mtEnumType, name)));
            }
        }

        /**********************************************************************************/

        public static bool TryResolveManifest(string manifestTypeName, out CrateManifestType manifestType)
        {
            int typeId;
            if (_nameMap.TryGetValue(manifestTypeName, out typeId))
            {
                manifestType = new CrateManifestType(manifestTypeName, typeId);
                return true;
            }

            manifestType = CrateManifestType.Unknown;
            return false;
        }

        public static bool TryResolveManifest(Type type, out CrateManifestType manifestType)
        {
            lock (GlobalTypeCacheLock)
            {
                if (ManifestsCache.TryGetValue(type, out manifestType))
                {
                    return manifestType != CrateManifestType.Unknown;
                }

                var manifestAttr = (CrateManifestTypeAttribute)type.GetCustomAttribute(typeof(CrateManifestTypeAttribute), false);

                if (manifestAttr == null || manifestAttr.ManifestType == null)
                {
                    if (typeof(Manifest).IsAssignableFrom(type) && !type.IsAbstract)
                    {
                        var sampleManifest = ((Manifest)Activator.CreateInstance(type));
                        manifestType = sampleManifest.ManifestType;
                    }
                    else
                    {
                        manifestType = CrateManifestType.Unknown;
                    }
                }
                else
                {
                    manifestType = manifestAttr.ManifestType;
                }

                ManifestsCache.Add(type, manifestType);

                return manifestType != CrateManifestType.Unknown;
            }
        }

        /**********************************************************************************/

        public static bool TryResolveManifest(object instance, out CrateManifestType manifestType)
        {
            if (instance == null)
            {
                manifestType = CrateManifestType.Unknown;
                return false;
            }
            
            var type = instance.GetType();
            var manifest = instance as Manifest;
            
            if (manifest != null)
            {
                lock (GlobalTypeCacheLock)
                {
                    if (!ManifestsCache.TryGetValue(type, out manifestType))
                    {
                        manifestType = manifest.ManifestType;
                        ManifestsCache.Add(type, manifestType);
                        return true;
                    }
                }
            }
            
            return TryResolveManifest(type, out manifestType);
        }

        /**********************************************************************************/
    }
}
