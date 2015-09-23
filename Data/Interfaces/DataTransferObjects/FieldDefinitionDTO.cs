using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class DropdownListFieldDefinitionDTO : FieldDefinitionDTO
    {
       
    }

    public class RadioButtonGroupFieldDefinitionDTO : FieldDefinitionDTO
    {
        [JsonProperty("groupName")]
        public string GroupName { get; set; }

        [JsonProperty("radios")]
        public List<RadioButton> Radios { get; set; }
    }

    public class FilterPaneFieldDefinitionDTO : FieldDefinitionDTO
    {
        [JsonProperty("fields")]
        public List<FilterPaneField> Fields { get; set;}
    }

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
        public List<FieldEvent> Events { get; set; }

        [JsonProperty("source")]
        public FieldSourceDTO Source { get; set; }
    }

    public class FieldSourceDTO
    {
        [JsonProperty("manifestType")]
        public string ManifestType { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }
    }

    public class FieldEvent
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("handler")]
        public string Handler { get; set; }

        public FieldEvent(string name, string handler)
        {
            Name = name;
            Handler = handler;
        }
    }

    public class RadioButton
    {
        [JsonProperty("selected")]
        public bool Selected { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class FilterPaneField
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
