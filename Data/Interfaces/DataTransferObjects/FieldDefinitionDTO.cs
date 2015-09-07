using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class FieldDefinitionDTO
    {
        public FieldDefinitionDTO() { }

        public FieldDefinitionDTO(string name, bool required, string value, string fieldLabel)
        {
            Type = TEXTBOX_FIELD;
            Name = name;
            Required = required;
            Value = value;
            FieldLabel = fieldLabel;
        }

        public FieldDefinitionDTO(string type, string name, bool required, string value, string fieldLabel)
        {
            Type = type;
            Name = name;
            Required = required;
            Value = value;
            FieldLabel = fieldLabel;
        }

        public const string CHECKBOX_FIELD = "checkboxField";
        public const string TEXTBOX_FIELD = "textField";

        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("required")]
        public bool Required { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("fieldLabel")]
        public string FieldLabel { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("selected")]
        public bool Selected { get; set; }

        [JsonProperty("events")]
        public string Events { get; set; }
    }
}
