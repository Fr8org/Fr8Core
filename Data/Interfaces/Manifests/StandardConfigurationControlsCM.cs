using System;
using System.Collections;
using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;
using Data.Constants;
using System.Linq;
using System.Reflection;
using Data.Crates;

namespace Data.Interfaces.Manifests
{
    /// <summary>
    /// This class represents manifest for action's configuraton controls. To simplify creation of the UI the following mechanism is integrated into this class.
    /// Developer can create its own class derived from StandardConfigurationControlsCM. This class will represent UI for a particular action. 
    /// In the constructor of this new class developer can initialize desired UI structure. 
    /// Developer can create define several properties in this class and assign certain UI controls to them.
    /// Example:
    /// public class ActionUi : StandardConfigurationControlsCM
    /// {
    ///     [JsonIgnore]
    ///     public TextBox SearchText { get; set; }
    ///     
    ///     [JsonIgnore]
    ///     public DropDownList Folder { get; set; }
    ///
    ///     [JsonIgnore]
    ///     public DropDownList Status { get; set; }
    ///
    ///     public ActionUi()
    ///     {
    ///         Controls = new List<ControlDefinitionDTO>();
    ///
    ///         Controls.Add(new TextArea
    ///         {
    ///             IsReadOnly = true,
    ///             Label = "",
    ///             Value = "<p>Search for DocuSign Envelopes where the following are true:</p>" +
    ///                     "<div>Envelope contains text:</div>"
    ///         });
    ///
    ///         Controls.Add((SearchText = new TextBox
    ///         {
    ///             Name = "SearchText",
    ///             Events = new List<ControlEvent> {ControlEvent.RequestConfig},
    ///         }));
    ///
    ///         Controls.Add((Folder = new DropDownList
    ///         {
    ///             Label = "Envelope is in folder:",
    ///             Name = "Folder",
    ///             Events = new List<ControlEvent> {ControlEvent.RequestConfig},
    ///             Source = new FieldSourceDTO(CrateManifestTypes.FieldDescription, "Folders")
    ///         }));
    ///
    ///         Controls.Add((Status = new DropDownList
    ///         {
    ///             Label = "Envelope has status:",
    ///             Name = "Status",
    ///             Events = new List<ControlEvent> {ControlEvent.RequestConfig},
    ///             Source = new FieldSourceDTO(CrateManifestTypes.FieldDescription, "Statuses")
    ///         }));
    ///     }
    /// }
    /// Name of the property must be the same as the name of the corresponding control.
    /// If you use this approach to define action's UI then it will be possible to access individual state of controls in the confinguration controls crate the following way:
    /// 
    ///  var ui = Crate.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();
    ///  var controls = new ActionUi();
    ///  controls.ClonePropertiesFrom(ui);
    ///  var mySearchTextBoxValue = ui.SearchText.Value;
    ///  var myDropDownListValue = ui.Folder.Value;
    /// 
    /// </summary>

    [CrateManifestSerializer(typeof (StandardConfigurationControlsSerializer))]
    public class StandardConfigurationControlsCM : Manifest
    {
        // Members of the StandardConfigurationControlsCM type that must be excluded during synchornization
        private static readonly HashSet<string> MembersToIgnore;

        public List<ControlDefinitionDTO> Controls { get; set; }

        static StandardConfigurationControlsCM()
        {
            MembersToIgnore = new HashSet<string>();

            // Exclude all public properties of StandardConfigurationControlsCM 
            foreach (var member in typeof (StandardConfigurationControlsCM).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                MembersToIgnore.Add(member.Name);
            }

            // Exclude all public fields of StandardConfigurationControlsCM 
            foreach (var member in typeof (StandardConfigurationControlsCM).GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                MembersToIgnore.Add(member.Name);
            }
        }

        public StandardConfigurationControlsCM()
            : base(MT.StandardConfigurationControls)
        {
            Controls = new List<ControlDefinitionDTO>();
        }

        public StandardConfigurationControlsCM(IEnumerable<ControlDefinitionDTO> controls)
            : this()
        {
            Controls.AddRange(controls);
        }

        public StandardConfigurationControlsCM(params ControlDefinitionDTO[] controls) : this(controls as IEnumerable<ControlDefinitionDTO>)
        {
        }

        // Find control by its name. Note, that this methods is no recursive
        public ControlDefinitionDTO FindByName(string name)
        {
            return Controls.SingleOrDefault(x => x.Name == name);
        }

        public T FindByName<T>(string name) where T : ControlDefinitionDTO
        {
            return (T) Controls.SingleOrDefault(x => x.Name == name);
        }

        // Find control of type T recusively.
        public T FindByNameNested<T>(string name)
        {
            foreach (var controlDefinitionDto in Controls)
            {
                var result = FindByNameRecurisve(controlDefinitionDto, name);
                if (result != null)
                {
                    return (T) result;
                }
            }

            return default(T);
        }

        // Sync controls properties from configuration controls crate with the current instance of StandardConfigurationControlsCM
        public void ClonePropertiesFrom(StandardConfigurationControlsCM configurationControls)
        {
            var type = GetType();

            // Clone properties
            foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (MembersToIgnore.Contains(prop.Name) || !prop.CanRead)
                {
                    continue;
                }

                var target = prop.GetValue(this);
                if (target == null)
                {
                    prop.SetValue(this, target = Activator.CreateInstance(prop.PropertyType));
                }
                ClonePropertiesForObject(target, prop.Name, configurationControls);
            }

            // Clone fields
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                if (MembersToIgnore.Contains(field.Name))
                {
                    continue;
                }

                ClonePropertiesForObject(field.GetValue(this), field.Name, configurationControls);
            }
        }

        // Clone properties from control with name 'name' into object 'target'
        private static void ClonePropertiesForObject(object target, string objectName, StandardConfigurationControlsCM configurationControls)
        {
            // Find the control
            var control = configurationControls.FindByNameNested<object>(objectName);

            if (control != null)
            {
                ClonePrimitiveProperties(target, control);
            }
        }

        // Clone properties from object 'source' to object 'target'
        private static void ClonePrimitiveProperties(object target, object source)
        {
            var properties = target.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => (x.PropertyType.IsValueType || x.PropertyType == typeof (string)) && x.CanWrite);
            var sourceTypeProp = source.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => (x.PropertyType.IsValueType || x.PropertyType == typeof (string)) && x.CanRead).ToDictionary(x => x.Name, x => x);

            foreach (var prop in properties)
            {
                PropertyInfo sourceProp;

                if (sourceTypeProp.TryGetValue(prop.Name, out sourceProp) && prop.PropertyType.IsAssignableFrom(sourceProp.PropertyType))
                {
                    try
                    {
                        prop.SetMethod.Invoke(target, new[] {sourceProp.GetMethod.Invoke(source, null)});
                    }
                    catch
                    {
                    }
                }
            }
        }

        // Check if the give instance of an object has apropriate name
        private bool CheckName(object control, string name)
        {
            // if control is ControlDefinitionDTO then just check Name property.
            var controlDef = control as ControlDefinitionDTO;

            if (controlDef != null && controlDef.Name == name)
            {
                return true;
            }

            // Not all our controls are derived from ControlDefinitionDTO. But these controls still has propery Name. Get it using the reflection.
            //TODO: much better solution is to introduce comonnd base class for every control, or indroduce interface, thaw will have property Name. Reflection shoudn't be used if there is a way to avoid it.
            var nameProp = control.GetType().GetProperty("Name", BindingFlags.Instance | BindingFlags.Public);

            if (nameProp == null || !nameProp.CanRead)
            {
                return false;
            }

            try
            {
                return (string) nameProp.GetMethod.Invoke(control, null) == name;
            }
            catch
            {
                return false;
            }
        }

        // Check if the given type is an either instance of IList or generic IList<>.
        private bool IsCompatibleCollectionType(Type type)
        {
            if (typeof (IList).IsAssignableFrom(type))
            {
                return true;
            }

            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof (IList<>);
        }

        // Find configuration control by name recursively.
        private object FindByNameRecurisve(object cd, string name)
        {
            // Check if current control has the desired name
            if (CheckName(cd, name))
            {
                return cd;
            }

            // if not, find all collection properties in the current control. Only properties of type IList or IList<> are supported. 
            var collectionGetters = cd.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => x.CanRead && IsCompatibleCollectionType(x.PropertyType)).ToArray();

            foreach (var collectionGetter in collectionGetters)
            {
                try
                {
                    var children = (IEnumerable) collectionGetter.GetMethod.Invoke(cd, null);
                    if (children == null)
                    {
                        continue;
                    }

                    foreach (var child in children)
                    {
                        var result = FindByNameRecurisve(child, name);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
                catch
                {
                }
            }

            return null;
        }
    }
}
