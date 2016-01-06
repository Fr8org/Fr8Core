using System;
using System.Collections.Generic;
using Data.Control;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    // TODO It will be good to change setter property 'Type' to protected to disallow change the type. We have all needed classes(RadioButtonGroupFieldDefinitionDTO, DropdownListFieldDefinitionDTO and etc).
    // But Wait_For_DocuSign_Event_v1.FollowupConfigurationResponse() directly write to this property !
    public class ControlDefinitionDTO : IResettable
    {
        public ControlDefinitionDTO() { }

        public ControlDefinitionDTO(string type)
        {
            Type = type;
        }
        
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("required")]
        public bool Required { get; set; }

        [JsonProperty("value")]
        public virtual string Value { get; set; }

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

        [JsonProperty("help")]
        public HelpControlDTO Help { get; set; }

        public virtual void Reset(List<string> fieldNames)
        {
            //This is here to prevent development bugs
            if (fieldNames != null)
            {
                throw new NotSupportedException();
            }
            Value = "";
        }
    }
}
