using System;
using System.Collections;
using System.Collections.Generic;
using Fr8Data.DataTransferObjects;
using Fr8Data.Constants;
using System.Linq;
using System.Reflection;
using Fr8Data.Crates;
using Fr8Data.Helpers;

namespace Fr8Data.Manifests
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

        public void Add(ControlDefinitionDTO control)
        {
            Controls.Add(control);
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

        public object FindByNameNested(string name)
        {
            foreach (var controlDefinitionDto in Controls)
            {
                var result = FindByNameRecurisve(controlDefinitionDto, name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public List<IControlDefinition> EnumerateControlsDefinitions()
        {
            var namedControls = new List<IControlDefinition>();

            foreach (var controlDefinitionDto in Controls)
            {
                EnumerateNamedControls(controlDefinitionDto, namedControls);
            }

            return namedControls;
        }

        private void EnumerateNamedControls(object obj, List<IControlDefinition> controls)
        {
            if (obj is IControlDefinition)
            {
                controls.Add((IControlDefinition)obj);
            }

            if (obj is IContainerControl)
            {
                foreach (var child in ((IContainerControl)obj).EnumerateChildren())
                {
                    EnumerateNamedControls(child, controls);
                }
            }
        }
        
        public void SyncWith(StandardConfigurationControlsCM configurationControls)
        {
            var targetNamedControls = EnumerateControlsDefinitions();
            foreach (var targetControl in targetNamedControls)
            {
                var source = configurationControls.FindByNameNested(targetControl.Name);

                if (source == null)
                {
                    continue;
                }

                ClonePrimitiveProperties(targetControl, source);
            }
        }
        // Sync controls properties from configuration controls crate with the current instance of StandardConfigurationControlsCM
        public StandardConfigurationControlsCM ClonePropertiesFrom(StandardConfigurationControlsCM configurationControls)
        {
            SyncWith(configurationControls);
            return this;
        }

        private static bool CheckIfTypeIsControlsCollection(Type type)
        {
            if (type.IsGenericType)
            {
                var genericTypeDef = type.GetGenericTypeDefinition();

                if (typeof(IList<>) == genericTypeDef)
                {
                    if (typeof(IControlDefinition).IsAssignableFrom(type.GetGenericArguments()[0]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        private static bool CanSyncMember(IMemberAccessor propertyInfo)
        {
            if (propertyInfo.Name == "Controls")
            {
                int wtf = 0;
            }

            if (propertyInfo.GetCustomAttribute<IgnorePropertySyncAttribute>() != null)
            {
                return false;
            }

            if (propertyInfo.GetCustomAttribute<ForcePropertySyncAttribute>() != null)
            {
                return true;
            }

            // if we have property of the type derived from IControlDefinition it 
            if (typeof (IControlDefinition).IsAssignableFrom(propertyInfo.MemberType))
            {
                return false;
            }

            if (propertyInfo.MemberType.IsInterface && CheckIfTypeIsControlsCollection(propertyInfo.MemberType))
            {
                return false;
            }

            foreach (var @interface in propertyInfo.MemberType.GetInterfaces())
            {
                if (CheckIfTypeIsControlsCollection(@interface))
                {
                    return false;
                }
            }
            
            return true;
        }

        private static IEnumerable<IMemberAccessor> GetMembers(Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Select(x =>  (IMemberAccessor) new PropertyMemberAccessor(x))
                .Concat(type.GetFields(BindingFlags.Instance | BindingFlags.Public).Select(x => (IMemberAccessor) new FieldMemberAccessor(x)));
        }

        // Clone properties from object 'source' to object 'target'
        private static void ClonePrimitiveProperties(object target, object source)
        {
            var members = GetMembers(target.GetType()).Where(x => CanSyncMember(x) && x.CanWrite);
            var sourceTypeProp = GetMembers(target.GetType()).Where(x=>CanSyncMember(x) && x.CanRead && x.CanWrite).ToDictionary(x => x.Name, x => x);

            foreach (var member in members)
            {
                IMemberAccessor sourceMember;

                if (sourceTypeProp.TryGetValue(member.Name, out sourceMember) && member.MemberType.IsAssignableFrom(sourceMember.MemberType))
                {
                    if (typeof (IList).IsAssignableFrom(sourceMember.MemberType))
                    {
                        if (!member.CanWrite)
                        {
                            var targetList = (IList)member.GetValue(target);
                            var sourceList = (IList)sourceMember.GetValue(source);

                            if (targetList != null)
                            {
                                targetList.Clear();

                                if (sourceList != null)
                                {
                                    foreach (var item in sourceList)
                                    {
                                        targetList.Add(item);
                                    }
                                }
                            }

                            return;
                        }
                    }

                    try
                    {
                        member.SetValue(target, sourceMember.GetValue(source));
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
            var namedControl = control as IControlDefinition;

            if (namedControl != null)
            {
                return namedControl.Name == name;
            }

            return false;
        }
        
        // Find configuration control by name recursively.
        private object FindByNameRecurisve(object cd, string name)
        {
            // Check if current control has the desired name
            if (CheckName(cd, name))
            {
                return cd;
            }

            var conatinerControl = cd as IContainerControl;

            if (conatinerControl != null)
            {
                foreach (var child in conatinerControl.EnumerateChildren())
                {
                    var result = FindByNameRecurisve(child, name);

                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }
    }
}
