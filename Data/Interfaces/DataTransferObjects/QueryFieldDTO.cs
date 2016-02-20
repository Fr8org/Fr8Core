using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Data.Interfaces.DataTransferObjects
{
    public class QueryFieldDTO
    {
        public QueryFieldDTO()
        {
        }

        public QueryFieldDTO(
            string name,
            string label,
            QueryFieldType fieldType,
            ControlDefinitionDTO control)
        {
            Name = name;
            Label = label;
            FieldType = fieldType;
            Control = control;
        }

        public string Name { get; set; }
        
        public string Label { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public QueryFieldType FieldType { get; set; }

        public ControlDefinitionDTO Control { get; set; }
    }
}
