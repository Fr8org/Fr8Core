using System.Collections;
using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;
using Data.Constants;
using System.Linq;
using System.Reflection;

namespace Data.Interfaces.ManifestSchemas
{
    public class StandardConfigurationControlsCM : Manifest
    {
        public List<ControlDefinitionDTO> Controls { get; set; }

        public StandardConfigurationControlsCM()
			  :base(MT.StandardConfigurationControls)
        {
            Controls = new List<ControlDefinitionDTO>();
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
             var props = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => x.CanWrite && x.CanRead && x.Name != "Controls").ToArray();

             foreach (var prop in props)
             {
                 var control = configurationControls.FindByNameNested<object>(prop.Name);
                 if (control != null)
                 {
                     ClonePrimitiveProperties(prop.GetValue(this), control);
                 }
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

        private object FindByNameRecurisve(object cd, string name)
        {
            if (CheckName(cd, name))
            {
                return cd;
            }

            var collectionGetters = cd.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => x.CanRead && typeof(IList).IsAssignableFrom(x.PropertyType)).ToArray();

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
                            return child;
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
