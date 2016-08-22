using System;
using System.Collections.Generic;
using System.Reflection;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Utilities;
using Hub.Interfaces;

namespace Hub.Services
{
    public class ManifestService : IManifest
    {
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

                crateDto = Crate.FromContent(clrManifestType.Name, new FieldDescriptionsCM(curFieldDto.ToArray()));
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
                        Name = property.Name,
                        Label = property.PropertyType.FullName
                    });
                }
                else
                {
                    curPropertiesList.Add(new FieldDTO()

                    {
                        Name = property.Name,
                        Label = property.PropertyType.Name,
                    });
                }

            }
            return curPropertiesList;
        }
    }
}
