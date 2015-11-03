using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Data.Crates
{
    public class ManifestDiscovery
    {
        /**********************************************************************************/
        // Declarations
        /**********************************************************************************/

        public static readonly ManifestDiscovery Default = new ManifestDiscovery();

        /**********************************************************************************/
        
        private readonly Dictionary<CrateManifestType, Type> _typeMapping = new Dictionary<CrateManifestType, Type>();
        
        /**********************************************************************************/
        // Functions
        /**********************************************************************************/

        private ManifestDiscovery()
        {
            ConfigureInitial();
        }

        /**********************************************************************************/

        public void ConfigureInitial()
        {
            var manifest = typeof(Interfaces.Manifests.Manifest);

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(x => manifest.IsAssignableFrom(x) || x.GetCustomAttribute<CrateManifestAttribute>() != null))
            {
                if (type.IsAbstract)
                {
                    continue;
                }

                RegisterManifest(type);
            }
        }

        /**********************************************************************************/

        public void RegiserManifest<T>()
        {
            RegisterManifest(typeof(T));
        }

        /**********************************************************************************/

        public bool TryResolveType(CrateManifestType manifestType, out Type type)
        {
            lock (_typeMapping)
            {
                return _typeMapping.TryGetValue(manifestType, out type);
            }
        }

        /**********************************************************************************/

        public void RegisterManifest(Type type)
        {
            CrateManifestType manifestType;

            if (!ManifestTypeCache.TryResolveManifest(type, out manifestType))
            {
                throw new ArgumentException("Type is not marked with CrateManifestAttribute or ManifestType is not set");
            }

            lock (_typeMapping)
            {
                _typeMapping[manifestType] = type;
            }
        }

        /**********************************************************************************/
    }
}
