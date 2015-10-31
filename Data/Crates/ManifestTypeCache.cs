using System;
using System.Collections.Generic;
using System.Reflection;
using Data.Interfaces.Manifests;

namespace Data.Crates
{
    internal static class ManifestTypeCache
    {
        /**********************************************************************************/
        // Declarations
        /**********************************************************************************/

        private static readonly Dictionary<Type, CrateManifestType> ManifestsCache = new Dictionary<Type, CrateManifestType>();
        private static readonly object GlobalTypeCacheLock = new object();
        
        /**********************************************************************************/

        public static bool TryResolveManifest(Type type, out CrateManifestType manifestType)
        {
            lock (GlobalTypeCacheLock)
            {
                if (ManifestsCache.TryGetValue(type, out manifestType))
                {
                    return manifestType != CrateManifestType.Unknown;
                }

                var manifestAttr = (CrateManifestAttribute)type.GetCustomAttribute(typeof(CrateManifestAttribute));

                if (manifestAttr == null || manifestAttr.ManifestType == null)
                {
                    if (typeof(Manifest).IsAssignableFrom(type))
                    {
                        var sampleManifest = ((Manifest)Activator.CreateInstance(type));
                        manifestType = new CrateManifestType(sampleManifest.ManifestName, sampleManifest.ManifestId);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    manifestType = CrateManifestType.FromEnum(manifestAttr.ManifestType);
                }
            
                ManifestsCache.Add(type, manifestAttr != null ? CrateManifestType.FromEnum(manifestAttr.ManifestType) : CrateManifestType.Unknown);

                return true;
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

            return TryResolveManifest(type, out manifestType);
        }

        /**********************************************************************************/
    }
}
