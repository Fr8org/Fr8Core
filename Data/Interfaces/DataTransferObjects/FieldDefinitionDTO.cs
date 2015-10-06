using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
	public class CheckBoxFieldDefinitionDTO : ControlsDefinitionDTO
	{
		public CheckBoxFieldDefinitionDTO()
		{
			Type = "checkboxField";
		}
	}
    public class DropdownListFieldDefinitionDTO : ControlsDefinitionDTO
    {
		 [JsonProperty("listItems")]
		 public List<ListItem> ListItems { get; set; }

		 public DropdownListFieldDefinitionDTO()
		 {
			 ListItems = new List<ListItem>();
			 Type = "dropdownlistField";
		 }
    }

    public class RadioButtonGroupFieldDefinitionDTO : ControlsDefinitionDTO
    {
        [JsonProperty("groupName")]
        public string GroupName { get; set; }

        [JsonProperty("radios")]
        public List<RadioButton> Radios { get; set; }

		 public RadioButtonGroupFieldDefinitionDTO()
		  {
			  Radios = new List<RadioButton>();
			  Type = "radioGroup";
		  }
    }

    public class FilterPaneFieldDefinitionDTO : ControlsDefinitionDTO
    {
        [JsonProperty("fields")]
        public List<FilterPaneField> Fields { get; set;}
		 
		 public FilterPaneFieldDefinitionDTO()
		  {
			  Type = "filterPane";
		  }
    }
	 // TODO It will be good to change setter property 'Type' to protected to disallow change the type. We have all needed classes(RadioButtonGroupFieldDefinitionDTO, DropdownListFieldDefinitionDTO and etc).
	 // But Wait_For_DocuSign_Event_v1.FollowupConfigurationResponse() directly write to this property !
    public class ControlsDefinitionDTO
    {
        public ControlsDefinitionDTO() { }

		  public ControlsDefinitionDTO(string type) 
		  {
			  Type = type;
		  }

        public ControlsDefinitionDTO(string name, bool required, string value, string fieldLabel)
        {
            Type = TEXTBOX_FIELD;
            Name = name;
            Required = required;
            Value = value;
            Label = fieldLabel;
        }

        public ControlsDefinitionDTO(string type, string name, bool required, string value, string fieldLabel)
        {
            Type = type;
            Name = name;
            Required = required;
            Value = value;
            Label = fieldLabel;
        }

        public const string CHECKBOX_FIELD = "checkboxField";
        public const string TEXTBOX_FIELD = "textField";

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("required")]
        public bool Required { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("type")]
        public string Type { get; protected set; }

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
	 public class ListItem
	 {
		 [JsonProperty("selected")]
		 public bool Selected { get; set; }

		 [JsonProperty("key")]
		 public string Key { get; set; }

		 [JsonProperty("value")]
		 public string Value { get; set; }
	 }
}
