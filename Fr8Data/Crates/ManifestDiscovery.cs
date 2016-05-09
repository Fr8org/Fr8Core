using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fr8Data.Manifests;

namespace Fr8Data.Crates
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
            var manifest = typeof(Manifest);

            foreach (var type in ListAssemblyTypes(Assembly.GetExecutingAssembly()).Where(x => manifest.IsAssignableFrom(x) || x.GetCustomAttribute<CrateManifestTypeAttribute>() != null))
            {
                if (type.IsAbstract || type == manifest)
                {
                    continue;
                }
                
                RegisterManifest(type);
            }
        }

        /**********************************************************************************/

        private static IEnumerable<Type> ListAssemblyTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(type => type != null);
            }
        }

        /**********************************************************************************/

        public void RegiserManifest<T>()
        {
            RegisterManifest(typeof(T));
        }

        /**********************************************************************************/

        public bool TryGetManifestType(Type type, out CrateManifestType manifestType)
        {
            return ManifestTypeCache.TryResolveManifest(type, out manifestType);
        }
        
        /**********************************************************************************/

        public CrateManifestType GetManifestType(Type type)
        {
            CrateManifestType manifestType;

            if (!ManifestTypeCache.TryResolveManifest(type, out manifestType))
            {
                throw new ArgumentException("Type is not marked with CrateManifestAttribute or ManifestType is not set");
            }

            return manifestType;
        }

        /**********************************************************************************/

        public CrateManifestType GetManifestType<T>()
        {
            return GetManifestType(typeof(T));
        }

        /**********************************************************************************/

        public bool TryResolveType(CrateManifestType manifestType, out Type type)
        {
            lock (_typeMapping)
            {
                return _typeMapping.TryGetValue(manifestType, out type);
            }
        }

        public bool TryResolveManifestType(string manifestTypeName, out CrateManifestType manifestType)
        {
            return ManifestTypeCache.TryResolveManifest(manifestTypeName, out manifestType);
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
