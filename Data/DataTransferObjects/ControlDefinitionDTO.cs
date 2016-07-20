using System;
using System.Collections.Generic;
using Fr8Data.Control;
using Newtonsoft.Json;

namespace Fr8Data.DataTransferObjects
{
    // We have logic that can copy properties from one StandardConfigurationControlsCM to another.
    // Important moment here is that we want to copy only properties (i.e what can be changed by user) no structure.
    // We don't want to suddedly insert new Control that exists in one StandardConfigurationControlsCM to another. It is very important in 
    // Easy way not modify structure is not to sync collection properties (at least until we don't have control that has property of type ControlDefinitionDTO).
    // But sometimes we have to sync collections. For example in case of UpstreamCrateChooser.SelectedCrates.
    // To force collection property synchronization we introduce this attribute
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    sealed class ForcePropertySyncAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    sealed class IgnorePropertySyncAttribute : Attribute
    {
    }

    public interface IControlDefinition
    {
        string Name { get; set; }
    }

    public interface IContainerControl
    {
        IEnumerable<IControlDefinition> EnumerateChildren();
    }

    // TODO It will be good to change setter property 'Type' to protected to disallow change the type. We have all needed classes(RadioButtonGroupFieldDefinitionDTO, DropdownListFieldDefinitionDTO and etc).
    // But Wait_For_DocuSign_Event_v1.FollowupConfigurationResponse() directly write to this property !
    public class ControlDefinitionDTO : IResettable, IControlDefinition
    {
        public ControlDefinitionDTO()
        {
            Events = new List<ControlEvent>();
        }

        public ControlDefinitionDTO(string type) : base()
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
        [IgnorePropertySync]
        public List<ControlEvent> Events { get; set; }

        [JsonProperty("source")]
        public FieldSourceDTO Source { get; set; }

        [JsonProperty("showDocumentation")]
        public ActivityResponseDTO ShowDocumentation { get; set; }
       
        [JsonProperty("isHidden")]
        public bool IsHidden { get; set; }

        [JsonProperty("isCollapsed")]
        public bool IsCollapsed  { get; set; }

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
