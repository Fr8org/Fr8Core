using System;
using System.Collections;
using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;
using Data.Constants;
using System.Linq;
using System.Reflection;
using Data.Control;
using Data.Crates;

namespace Data.Interfaces.Manifests
{
    [CrateManifestSerializer(typeof(StandardConfigurationControlsSerializer))]
    public class StandardConfigurationControlsCM : Manifest
    {
        private static readonly HashSet<string> MembersToIgnore;

        public List<ControlDefinitionDTO> Controls { get; set; }

        static StandardConfigurationControlsCM()
        {
            MembersToIgnore = new HashSet<string>();

            foreach (var member in typeof(StandardConfigurationControlsCM).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                MembersToIgnore.Add(member.Name);
            }

            foreach (var member in typeof(StandardConfigurationControlsCM).GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                MembersToIgnore.Add(member.Name);
            }
        }

        public StandardConfigurationControlsCM()
			  :base(MT.StandardConfigurationControls)
        {
            Controls = new List<ControlDefinitionDTO>();
        }

        public StandardConfigurationControlsCM(IEnumerable<ControlDefinitionDTO> controls)
            :this()
        {
            Controls.AddRange(controls);
        }

        public ControlDefinitionDTO FindByName(string name)
		  {
			  return Controls.SingleOrDefault(x => x.Name == name);
		  }

         public T FindByNameNested<T>(string name)
         {
             foreach (var controlDefinitionDto in Controls)
             {
                 var result = FindByNameRecurisve(controlDefinitionDto, name);
                 if (result != null)
                 {
                     return (T)result;
                 }
             }

             return default(T);
         }

         public void ClonePropertiesFrom(StandardConfigurationControlsCM configurationControls)
         {
             var type = GetType();

             foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
             {
                 if (MembersToIgnore.Contains(prop.Name) || !prop.CanRead)
                 {
                     continue;
                 }

                 ClonePropertiesForObject(prop.GetValue(this), prop.Name, configurationControls);
             }

             foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
             {
                 if (MembersToIgnore.Contains(field.Name))
                 {
                     continue;
                 }

                 ClonePropertiesForObject(field.GetValue(this), field.Name, configurationControls);
             }
         }

        private static void ClonePropertiesForObject(object target, string objectName, StandardConfigurationControlsCM configurationControls)
        {
            var control = configurationControls.FindByNameNested<object>(objectName);
            
            if (control != null)
            {
                ClonePrimitiveProperties(target, control);
            }
        }

        private static void ClonePrimitiveProperties(object target, object source)
         {
             var properties = target.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => (x.PropertyType.IsValueType || x.PropertyType == typeof(string)) && x.CanWrite);
             var sourceTypeProp = source.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => (x.PropertyType.IsValueType || x.PropertyType == typeof(string)) && x.CanRead).ToDictionary(x => x.Name, x => x);

             foreach (var prop in properties)
             {
                 PropertyInfo sourceProp;

                 if (sourceTypeProp.TryGetValue(prop.Name, out sourceProp) && prop.PropertyType.IsAssignableFrom(sourceProp.PropertyType))
                 {
                     try
                     {
                         prop.SetMethod.Invoke(target, new[] { sourceProp.GetMethod.Invoke(source, null) });
                     }
                     catch
                     { }
                 }
             }
         }

        private bool CheckName(object control, string name)
        {
            var controlDef = control as ControlDefinitionDTO;

            if (controlDef != null && controlDef.Name == name)
            {
                return true;
            }

            var nameProp = control.GetType().GetProperty("Name", BindingFlags.Instance | BindingFlags.Public);

            if (nameProp == null || !nameProp.CanRead)
            {
                return false;
            }

            try
            {
                return (string)nameProp.GetMethod.Invoke(control, null) == name;
            }
            catch
            {
                return false;
            }
        }
        
        private bool IsCompatibleCollectionType(Type type)
        {
            if (typeof(IList).IsAssignableFrom(type))
            {
                return true;
            }

            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof (IList<>);
        }
        
        private object FindByNameRecurisve(object cd, string name)
        {
            if (CheckName(cd, name))
            {
                return cd;
            }

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
