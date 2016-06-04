using System;
using System.Collections.Generic;
using System.Reflection;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Hub.Interfaces;
using Hub.Managers;
using StructureMap;
using Utilities;

namespace Hub.Services
{
    public class ManifestService : IManifest
    {
        private readonly ICrateManager _curCrateManager;

        public ManifestService()
        {
            _curCrateManager = ObjectFactory.GetInstance<ICrateManager>();
        }

        // Use the reflection and get the properties of manifest class. 
        // Create the designTime fields from fetched properties and send it to client.
        public Crate GetById(int id)
        {
            Crate crateDto = null;
            Type clrManifestType;

            if (ManifestDiscovery.Default.TryResolveType(new CrateManifestType(null, id), out clrManifestType))
            {
                var propertyInfo = ReflectionHelper.GetProperties(clrManifestType);
                var curFieldDto = ConvertPropertyToFields(propertyInfo);

                crateDto = _curCrateManager.CreateDesignTimeFieldsCrate(clrManifestType.Name, curFieldDto.ToArray());
            }

            return crateDto;
        }

        // Convert all properties to FieldDTO
        public List<FieldDTO> ConvertPropertyToFields( PropertyInfo[]  curProperties)
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
