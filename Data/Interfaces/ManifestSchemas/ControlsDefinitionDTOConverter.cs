using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace Data.Interfaces.ManifestSchemas
{
    public class ControlsDefinitionDTOConverter : CustomCreationConverter<ControlDefinitionDTO>
    {
        public override ControlDefinitionDTO Create(Type objectType)
        {
            return new GenericControlDefinitionDTO();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Create a map of all properties in the field object
            var propertyMap = createPropertyMap(reader);
            string fieldTypeName = string.Empty;
            Type fieldType;

            // Find field type 
            if (!propertyMap.ContainsKey("type"))
            {
                fieldTypeName = propertyMap["type"];

                // Determine field .Net type depending on type value 
                fieldType = GetFieldType(fieldTypeName);
            }

            return null;
        }

        private Type GetFieldType(string fieldTypeName)
        {
            return null;
        }

        private Dictionary<string, string> createPropertyMap(JsonReader reader)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            string propName = String.Empty, propValue = String.Empty;
       
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    propName = reader.Value.ToString().ToLower();
                    // Find field value
                    if (reader.Read())
                    {
                        propValue = reader.Value.ToString().ToLower();
                        map.Add(propName, propValue);
                    }
                }
            }
            return map;
        }
    }

}
