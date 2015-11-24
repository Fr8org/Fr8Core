using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Data.Crates;
using Data.Interfaces.Manifests;

namespace Data.Interfaces.DataTransferObjects
{
    public class ControlDefinitionDTOConverter : CustomCreationConverter<ControlDefinitionDTO>
    {
        public override ControlDefinitionDTO Create(Type objectType)
        {
            return new GenericControlDefinitionDTO();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Type controlType;
            var curjObject = JObject.ReadFrom(reader);

            // Create a map of all properties in the field object
            string fieldTypeName =GetControlTypeName(curjObject);

            // Determine field .Net type depending on type value 
            controlType = GetFieldType(fieldTypeName);

            // Create type
            if (controlType == null)
            {
                controlType = typeof(GenericControlDefinitionDTO);
            }

            var control = Activator.CreateInstance(controlType);
            if (control == null) control = new GenericControlDefinitionDTO();
            serializer.Populate(curjObject.CreateReader(), control);

            return control;
        }

        private string GetControlTypeName(JToken curjObject)
        {
            var typeProperty = curjObject.Children<JProperty>().Where(p => p.Name == "type").FirstOrDefault();
            if (typeProperty == null)
            {
                return null;
            }

            return typeProperty.Value.Value<string>();

        }

        private Type GetFieldType(string fieldTypeName)
        {
            try
            {
                return Type.GetType(string.Format("Data.Interfaces.DataTransferObjects.{0}ControlDefinitionDTO, Data", fieldTypeName));
            }
            catch
            {
                return null;
            }
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


    public class StandardConfigurationControlsSerializer : IManifestSerializer
    {
        public void Initialize(ICrateStorageSerializer manager)
        {
        }

        public object Deserialize(JToken crateContent)
        {
            var converter = new ControlDefinitionDTOConverter();

            var serializer = JsonSerializer.Create(new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>
                {
                    converter
                }
            });

            return crateContent.ToObject<StandardConfigurationControlsCM>(serializer);
        }

        public JToken Serialize(object content)
        {
            return JToken.FromObject(content);
        }
    }
}
