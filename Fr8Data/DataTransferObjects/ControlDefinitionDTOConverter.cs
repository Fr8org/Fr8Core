using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.Manifests;

namespace Fr8Data.DataTransferObjects
{

    public class ControlMetaDescriptionDTOConverter : CustomCreationConverter<ControlMetaDescriptionDTO>
    {
        public override ControlMetaDescriptionDTO Create(Type objectType)
        {
            return new TextBoxMetaDescriptionDTO();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Type controlType;
            var curjObject = JObject.ReadFrom(reader);

            if (!curjObject.HasValues)
            {
                return null;
            }

            // Create a map of all properties in the field object
            string fieldTypeName = GetControlTypeName(curjObject);

            // Determine field .Net type depending on type value 
            controlType = GetFieldType(fieldTypeName);

            // Create type
            if (controlType == null)
            {
                controlType = typeof(TextBoxMetaDescriptionDTO);
            }

            var control = Activator.CreateInstance(controlType) ?? new TextBoxMetaDescriptionDTO();
            serializer.Populate(curjObject.CreateReader(), control);

            return control;
        }

        private string GetControlTypeName(JToken curjObject)
        {
            var typeProperty = curjObject.Children<JProperty>().FirstOrDefault(p => p.Name == "type");
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
                return Type.GetType(string.Format("Fr8Data.Control.{0}, Fr8Data", fieldTypeName));
            }
            catch
            {
                return null;
            }
        }

    }

    public class ControlDefinitionDTOConverter : CustomCreationConverter<ControlDefinitionDTO>
    {
        public override ControlDefinitionDTO Create(Type objectType)
        {
            return new Generic();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Type controlType;
            var curjObject = JObject.ReadFrom(reader);

            if (!curjObject.HasValues)
            {
                return null;
            }

            // Create a map of all properties in the field object
            string fieldTypeName = GetControlTypeName(curjObject);

            // Determine field .Net type depending on type value 
            controlType = GetFieldType(fieldTypeName);

            // Create type
            if (controlType == null)
            {
                controlType = typeof(Generic);
            }

            var control = Activator.CreateInstance(controlType) ?? new Generic();
            serializer.Populate(curjObject.CreateReader(), control);

            return control;
        }

        private string GetControlTypeName(JToken curjObject)
        {
            var typeProperty = curjObject.Children<JProperty>().FirstOrDefault(p => p.Name == "type");
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
                return Type.GetType(string.Format("Fr8Data.Control.{0}, Fr8Data", fieldTypeName));
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
