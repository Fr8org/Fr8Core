using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    /// <summary>
    /// This interface is applied to controls and control data items (e.g. radio buttons)
    /// that support nested controls.
    /// </summary>
    public interface ISupportsNestedFields
    {
        IList<ControlDefinitionDTO> Controls { get; }
    }

    public class ControlTypes
    {
        public const string TextBox = "TextBox";
        public const string CheckBox = "CheckBox";
        public const string DropDownList = "DropDownList";
        public const string RadioButtonGroup = "RadioButtonGroup";
        public const string FilterPane = "FilterPane";
        public const string MappingPane = "MappingPane";
        public const string TextBlock = "TextBlock";
        public const string FilePicker = "FilePicker";
        public const string Routing = "Routing";
    }

    public class CheckBoxControlDefinitionDTO : ControlDefinitionDTO
    {
        public CheckBoxControlDefinitionDTO()
        {
            Type = ControlTypes.CheckBox;
        }
    }
    public class DropDownListControlDefinitionDTO : ControlDefinitionDTO
    {
        [JsonProperty("listItems")]
        public List<ListItem> ListItems { get; set; }

        public DropDownListControlDefinitionDTO()
        {
            ListItems = new List<ListItem>();
            Type = "DropDownList";
        }
    }

    public class RadioButtonGroupControlDefinitionDTO : ControlDefinitionDTO
    {
        [JsonProperty("groupName")]
        public string GroupName { get; set; }

        [JsonProperty("radios")]
        public List<RadioButtonOption> Radios { get; set; }

        public RadioButtonGroupControlDefinitionDTO()
        {
            Radios = new List<RadioButtonOption>();
            Type = ControlTypes.RadioButtonGroup;
        }
    }

    public class TextBoxControlDefinitionDTO : ControlDefinitionDTO
    {
        public TextBoxControlDefinitionDTO()
        {
            Type = ControlTypes.TextBox;
        }
    }

    public class FilterPaneControlDefinitionDTO : ControlDefinitionDTO
    {
        [JsonProperty("fields")]
        public List<FilterPaneField> Fields { get; set; }

        public FilterPaneControlDefinitionDTO()
        {
            Type = ControlTypes.FilterPane;
        }
    }

    public class MappingPaneControlDefinitionDTO : ControlDefinitionDTO
    {
        public MappingPaneControlDefinitionDTO()
        {
            Type = ControlTypes.MappingPane;
        }
    }

    public class GenericControlDefinitionDTO : ControlDefinitionDTO
    {
        public GenericControlDefinitionDTO()
        {
            Type = ControlTypes.TextBox; // Yes, default to TextBox
        }
    }

    public class TextBlockControlDefinitionDTO : ControlDefinitionDTO
    {
        [JsonProperty("class")]
        public string CssClass;

        public TextBlockControlDefinitionDTO()
        {
            Type = ControlTypes.TextBlock;
        }
    }


    // TODO It will be good to change setter property 'Type' to protected to disallow change the type. We have all needed classes(RadioButtonGroupFieldDefinitionDTO, DropdownListFieldDefinitionDTO and etc).
    // But Wait_For_DocuSign_Event_v1.FollowupConfigurationResponse() directly write to this property !
    public class ControlDefinitionDTO
    {
        public ControlDefinitionDTO() { }

        public ControlDefinitionDTO(string type)
        {
            Type = type;
        }

        //public ControlsDefinitionDTO(string name, bool required, string value, string fieldLabel)
        //{
        //    Type = "textField";
        //    Name = name;
        //    Required = required;
        //    Value = value;
        //    Label = fieldLabel;
        //}

        //public ControlsDefinitionDTO(string type, string name, bool required, string value, string fieldLabel)
        //{
        //    Type = type;
        //    Name = name;
        //    Required = required;
        //    Value = value;
        //    Label = fieldLabel;
        //}

        //public const string CHECKBOX_FIELD = "checkboxField";
        //public const string TEXTBOX_FIELD = "textField";

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
        public List<ControlEvent> Events { get; set; }

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

    public class ControlEvent
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("handler")]
        public string Handler { get; set; }

        public ControlEvent(string name, string handler)
        {
            Name = name;
            Handler = handler;
        }
    }

    public class RadioButtonOption : ISupportsNestedFields
    {
        public RadioButtonOption()
        {
            Controls = new List<ControlDefinitionDTO>();
        }

        [JsonProperty("selected")]
        public bool Selected { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("controls")]
        public IList<ControlDefinitionDTO> Controls { get; set; }
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
