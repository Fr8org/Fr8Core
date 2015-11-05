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

                var manifestAttr = (CrateManifestTypeAttribute)type.GetCustomAttribute(typeof(CrateManifestTypeAttribute));

                if (manifestAttr == null || manifestAttr.ManifestType == null)
                {
                    if (typeof(Manifest).IsAssignableFrom(type))
                    {
                        var sampleManifest = ((Manifest)Activator.CreateInstance(type));
                        manifestType = new CrateManifestType(sampleManifest.ManifestName, sampleManifest.ManifestId);
                    }
                    else
                    {
                        manifestType = CrateManifestType.Unknown;
                    }
                }
                else
                {
                    manifestType = CrateManifestType.FromEnum(manifestAttr.ManifestType);
                }

                ManifestsCache.Add(type, manifestType);

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
            var manifest = instance as Manifest;
            
            if (manifest != null)
            {
                lock (GlobalTypeCacheLock)
                {
                    if (!ManifestsCache.TryGetValue(type, out manifestType))
                    {
                        manifestType = new CrateManifestType(manifest.ManifestName, (int) manifest.ManifestType);
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
