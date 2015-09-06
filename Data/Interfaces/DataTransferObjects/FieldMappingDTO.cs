using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Data.Interfaces.DataTransferObjects
{
    public class FieldMappingDTO
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public abstract class MappingDTOBase : List<FieldMappingDTO>
    {
        protected abstract string _rootName { get; }

        /// <summary>
        /// Serializes the current object to JSON.
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            JObject payload = new JObject();
            ForEach(m =>
            {
                JProperty propMapping = new JProperty(m.Name, m.Value);
                payload.Add(propMapping);
            });
            JObject result = new JObject(new JProperty(_rootName, payload));
            return result.ToString();
        }

        /// <summary>
        /// Deserializes the provided JSON string to the current instance.
        /// </summary>
        /// <param name="json"></param>
        public void Deserialize(string json)
        {
            JObject mappings = JObject.Parse(json)[_rootName] as JObject;
            foreach (JProperty prop in mappings.Properties())
            {
                Add(new FieldMappingDTO()
                {
                    Name = prop.Name,
                    Value = prop.Value.ToString()
                });
            }
        }
    }

    public class FieldMappingSettingsDTO
    {
        [JsonProperty("fields")]
        public List<FieldMappingDTO> Fields { get; set; }
    }

    public class PayloadMappingsDTO : MappingDTOBase
    {
        protected override string _rootName
        {
            get
            {
                return "payload";
            }
        }
    }
}
