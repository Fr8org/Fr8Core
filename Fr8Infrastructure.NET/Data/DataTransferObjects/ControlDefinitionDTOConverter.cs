using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.Manifests;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{

    public class ControlMetaDescriptionDTOConverter : CustomCreationConverter<ControlMetaDescriptionDTO>
    {
        public override ControlMetaDescriptionDTO Create(Type objectType)
        {
            return new TextBoxMetaDescriptionDTO();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var curjObject = JToken.ReadFrom(reader);

            if (!curjObject.HasValues)
            {
                return null;
            }

            // Create a map of all properties in the field object
            var fieldTypeName = GetControlTypeName(curjObject);

            // Determine field .Net type depending on type value 
            var controlType = GetFieldType(fieldTypeName);

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
            return typeProperty?.Value.Value<string>();

        }

        private Type GetFieldType(string fieldTypeName)
        {
            try
            {
                return Type.GetType($"Fr8.Infrastructure.Data.Control.{fieldTypeName}, Fr8Infrastructure.NET");
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
            var curjObject = JToken.ReadFrom(reader);

            if (!curjObject.HasValues)
            {
                return null;
            }

            // Create a map of all properties in the field object
            var fieldTypeName = GetControlTypeName(curjObject);

            // Determine field .Net type depending on type value 
            var controlType = GetFieldType(fieldTypeName) ?? typeof(Generic);
            // Create type
            var control = Activator.CreateInstance(controlType) ?? new Generic();
            serializer.Populate(curjObject.CreateReader(), control);

            return control;
        }

        private string GetControlTypeName(JToken curjObject)
        {
            var typeProperty = curjObject.Children<JProperty>().FirstOrDefault(p => p.Name == "type");
            return typeProperty?.Value.Value<string>();

        }

        private Type GetFieldType(string fieldTypeName)
        {
            try
            {
                return Type.GetType(string.Format("Fr8.Infrastructure.Data.Control.{0}, Fr8Infrastructure.NET", fieldTypeName));
            }
            catch
            {
                return null;
            }
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
