using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;
using Hub.Managers;
using Data.Interfaces;
using Data.Interfaces.Manifests;
using System.Reflection;
using Hub.Interfaces;
using Newtonsoft.Json;
using StructureMap;
using Utilities;

namespace Hub.Services
{
    public class Manifest : IManifest
    {
        private readonly ICrateManager _curCrateManager;
        private readonly Dictionary<int, string> _curManifestDictionary;

        public Manifest()
        {
            _curCrateManager = ObjectFactory.GetInstance<ICrateManager>();
            _curManifestDictionary = CrateManifests.MANIFEST_CLASS_MAPPING_DICTIONARY;
        }

        // Use the reflection and get the properties of manifest class. 
        // Create the designTime fields from fetched properties and send it to client.
        public CrateDTO GetById(int id)
        {
            CrateDTO crateDTO = null;
            string manifestAssemblyName = null;

            _curManifestDictionary.TryGetValue(id, out manifestAssemblyName);

            if (!String.IsNullOrWhiteSpace(manifestAssemblyName))
            {
                var curAssemblyName = "Data";
                string fullyQualifiedName = string.Format("{0}.Interfaces.Manifests.{1}", curAssemblyName, manifestAssemblyName);
                Assembly assembly = Assembly.Load(curAssemblyName);
                Type cuAssemblyType = assembly.GetType(fullyQualifiedName);

                if (cuAssemblyType == null)
                    throw new ArgumentException(manifestAssemblyName);

                PropertyInfo[] propertyInfo = ReflectionHelper.GetProperties(cuAssemblyType);
                List<FieldDTO> curFieldDTO = convertPropertyToFields(propertyInfo);

                crateDTO = _curCrateManager.CreateDesignTimeFieldsCrate(manifestAssemblyName, curFieldDTO.ToArray());
            }

            return crateDTO;
        }

        // Convert all properties to FieldDTO
        public List<FieldDTO> convertPropertyToFields( PropertyInfo[]  curProperties)
        {
            var curPropertiesList = new List<FieldDTO>();
            foreach (var property in curProperties)
            {
                if (property.PropertyType.IsGenericType)
                {
                    curPropertiesList.Add(new FieldDTO()
                    {
                        Key = property.Name,
                        Value = property.PropertyType.FullName
                    });
                }
                else
                {
                    curPropertiesList.Add(new FieldDTO()

                    {
                        Key = property.Name,
                        Value = property.PropertyType.Name,
                    });
                }

            }
            return curPropertiesList;
        }
    }
}
