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

        public string Name { get; set; }
        
        public bool Required { get; set; }

        public string Value { get; set; }

        public string FieldLabel { get; set; }

        public string Type { get; set; }

        public bool Selected { get; set; }

        public List<FieldEvent> Events { get; set; }
    }

    public class FieldEvent
    {
        public string Name { get; set; }
        public string Handler { get; set; }

        public FieldEvent(string name, string handler)
        {
            Name = name;
            Handler = handler;
        }
    }
}
