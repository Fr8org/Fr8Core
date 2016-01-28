using System;
using System.Collections.Generic;
using System.Linq;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Newtonsoft.Json;

namespace Data.Control
{
    /// <summary>
    /// This interface is applied to controls and control data items (e.g. radio buttons)
    /// that support nested controls.
    /// </summary>
    public interface ISupportsNestedFields
    {
        IList<ControlDefinitionDTO> Controls { get; }
    }

    public interface IResettable
    {
        void Reset(List<string> fieldNames = null);
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
        public const string FieldList = "FieldList";
        public const string Button = "Button";
        public const string TextSource = "TextSource";
        public const string TextArea = "TextArea";
        public const string QueryBuilder = "QueryBuilder";
        public const string ManageRoute = "ManageRoute";
        public const string Duration = "Duration";
        public const string RunRouteButton = "RunRouteButton";
        public const string UpstreamDataChooser = "UpstreamDataChooser";
        public const string UpstreamFieldChooser = "UpstreamFieldChooser";
    }

    public class CheckBox : ControlDefinitionDTO
    {
        public CheckBox()
        {
            Type = ControlTypes.CheckBox;
        }
    }

    public class RunRouteButton : ControlDefinitionDTO
    {
        public RunRouteButton()
        {
            Type = ControlTypes.RunRouteButton;
        }
    }

    public class DropDownList : ControlDefinitionDTO
    {
        [JsonProperty("listItems")]
        public List<ListItem> ListItems { get; set; }

        [JsonProperty("selectedKey")]
        public string selectedKey { get; set; }

        public DropDownList() : base()
        {
            ListItems = new List<ListItem>();
            Type = "DropDownList";
        }
    }

    public class RadioButtonGroup : ControlDefinitionDTO
    {
        [JsonProperty("groupName")]
        public string GroupName { get; set; }

        [JsonProperty("radios")]
        public List<RadioButtonOption> Radios { get; set; }

        public RadioButtonGroup()
        {
            Radios = new List<RadioButtonOption>();
            Type = ControlTypes.RadioButtonGroup;
        }
    }

    public class TextBox : ControlDefinitionDTO
    {
        public TextBox()
        {
            Type = ControlTypes.TextBox;
        }
    }

    public class QueryBuilder : ControlDefinitionDTO
    {
        public QueryBuilder()
        {
            Type = ControlTypes.QueryBuilder;
        }
    }

    public class FilterPane : ControlDefinitionDTO
    {
        [JsonProperty("fields")]
        public List<FilterPaneField> Fields { get; set; }

        public FilterPane()
        {
            Type = ControlTypes.FilterPane;
        }
    }

    public class MappingPane : ControlDefinitionDTO
    {
        public MappingPane()
        {
            Type = ControlTypes.MappingPane;
        }
    }

    public class Generic : ControlDefinitionDTO
    {
        public Generic()
        {
            Type = ControlTypes.TextBox; // Yes, default to TextBox
        }
    }

    public class TextArea : ControlDefinitionDTO
    {
        [JsonProperty("isReadOnly")]
        public bool IsReadOnly { get; set; }

        public TextArea() :
            base(ControlTypes.TextArea)
        {
        }
    }

    public class TextBlock : ControlDefinitionDTO
    {
        [JsonProperty("class")]
        public string CssClass
        {
            get;
            set;
        }

        public TextBlock()
        {
            Type = ControlTypes.TextBlock;
        }
    }

    public class FilePicker : ControlDefinitionDTO
    {
        public FilePicker()
        {
            Type = ControlTypes.FilePicker;
        }
    }

    public class FieldList : ControlDefinitionDTO
    {
        public FieldList()
        {
            Type = ControlTypes.FieldList;
        }

        public override void Reset(List<string> fieldNames)
        {
            if (fieldNames != null)
            {
                //key-value pairs are immutable, we need to crate a new List
                var newList = new List<KeyValuePair<string, string>>();
                var keyValuePairs = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(Value);
                foreach (var keyValuePair in keyValuePairs)
                {
                    if (fieldNames.Any(n => n == keyValuePair.Key))
                    {
                        newList.Add(new KeyValuePair<string, string>(keyValuePair.Key, ""));
                    }
                    else
                    {
                        newList.Add(keyValuePair);
                    }
                }
                Value = JsonConvert.SerializeObject(newList);
            }
            else
            {
                Value = "[]";
            }
        }
    }

    public class TextSource : DropDownList
    {
        [JsonProperty("initialLabel")]
        public string InitialLabel;

        [JsonProperty("upstreamSourceLabel")]
        public string UpstreamSourceLabel;

        [JsonProperty("textValue")]
        public string TextValue;

        [JsonProperty("valueSource")]
        public string ValueSource;

        public TextSource() : base()
        {
            Type = ControlTypes.TextSource;
        }

        public TextSource(string initialLabel, string upstreamSourceLabel, string name) : base()
        {
            Type = ControlTypes.TextSource;
            this.InitialLabel = initialLabel;
            this.Name = name;
            Source = new FieldSourceDTO
            {
                Label = upstreamSourceLabel,
                ManifestType = CrateManifestTypes.StandardDesignTimeFields
            };
        }

        public string GetValue(CrateStorage payloadCrateStorage)
        {
            switch (ValueSource)
            {
                case "specific":
                    return TextValue;

                case "upstream":
                    return ExtractPayloadFieldValue(payloadCrateStorage);

                default:
                    throw new ApplicationException("Could not extract recipient, unknown recipient mode.");
            }
        }

        /// <summary>
        /// Extracts crate with specified label and ManifestType = Standard Design Time,
        /// then extracts field with specified fieldKey.
        /// </summary>
        private string ExtractPayloadFieldValue(CrateStorage payloadCrateStorage)
        {
            var fieldValues = payloadCrateStorage.CratesOfType<StandardPayloadDataCM>().SelectMany(x => x.Content.GetValues(selectedKey))
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();

            if (fieldValues.Length > 0)
                return fieldValues[0];

            throw new ApplicationException("No field found with specified key.");
        }
    }

    public class Button : ControlDefinitionDTO
    {
        [JsonProperty("class")]
        public string CssClass;

        /// <summary>
        /// Where the button was clicked before the current /configure request was sent.
        /// Used to recognize 'click' event on server-side.
        /// </summary>
        [JsonProperty("clicked")]
        public bool Clicked;

        public Button()
        {
            Type = ControlTypes.Button;
        }
    }

    public class Duration : ControlDefinitionDTO
    {
        public Duration()
        {
            Type = ControlTypes.Duration;
        }

        [JsonProperty("value")]
        public new TimeSpan Value
        {
            get
            {
                return new TimeSpan(this.Days, this.Hours, this.Minutes, 0);
            }
        }

        [JsonProperty("days")]
        public Int32 Days { get; set; }

        [JsonProperty("hours")]
        public Int32 Hours { get; set; }

        [JsonProperty("minutes")]
        public Int32 Minutes { get; set; }

    }




    public class FieldSourceDTO
    {
        [JsonProperty("manifestType")]
        public string ManifestType { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("filterByTag")]
        public string FilterByTag { get; set; }

        public FieldSourceDTO()
        {
        }

        public FieldSourceDTO(string manifestType, string label)
        {
            ManifestType = manifestType;
            Label = label;
        }
    }

    public class ControlEvent
    {
        public static ControlEvent RequestConfig
        {
            get
            {
                return new ControlEvent("onChange", "requestConfig");
            }
        }

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("handler")]
        public string Handler { get; set; }

        public ControlEvent(string name, string handler)
        {
            Name = name;
            Handler = handler;
        }

        public ControlEvent()
        {
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

    public class UpstreamDataChooser : ControlDefinitionDTO
    {
        public UpstreamDataChooser()
        {
            Type = ControlTypes.UpstreamDataChooser;
        }

        [JsonProperty("selectedManifest")]
        public string SelectedManifest { get; set; }

        [JsonProperty("selectedLabel")]
        public string SelectedLabel { get; set; }

        [JsonProperty("selectedFieldType")]
        public string SelectedFieldType { get; set; }
    }

    public class UpstreamFieldChooser : ControlDefinitionDTO
    {
        public UpstreamFieldChooser()
        {
            Type = ControlTypes.UpstreamFieldChooser;
        }
    }

    public class HelpControlDTO
    {
        public HelpControlDTO(string helpPath, string documentationSupport)
        {
            this.HelpPath = helpPath;
            this.DocumentationSupport = documentationSupport;
        }

        [JsonProperty("helpPath")]
        public string HelpPath { get; set; }

        [JsonProperty("documentationSupport")]
        public string DocumentationSupport { get; set; }
    }
}
