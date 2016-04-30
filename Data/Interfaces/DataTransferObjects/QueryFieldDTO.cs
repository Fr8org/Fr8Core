using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Data.Interfaces.DataTransferObjects
{
    // TODO: FR-3003, remove this.
    public class TypedFieldDTO
    {
        public TypedFieldDTO()
        {
        }
    
        public TypedFieldDTO(
            string name,
            string label,
            FieldType fieldType,
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
        public FieldType FieldType { get; set; }
    
        public ControlDefinitionDTO Control { get; set; }
    }
}
