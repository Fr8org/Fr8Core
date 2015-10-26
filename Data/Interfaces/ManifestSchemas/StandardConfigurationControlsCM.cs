using System.Collections;
using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;
using Data.Constants;
using Utilities;
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

        private object FindByNameRecurisve(object cd, string name)
        {
            var controlDef = cd as ControlDefinitionDTO;

            if (controlDef != null && controlDef.Name == name)
            {
                return cd;
            }
            
            var props = cd.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var nameProp = props.FirstOrDefault(x => x.CanRead && x.PropertyType == typeof(string) && x.Name == "Name");

            if (nameProp != null)
            {
                try
                {
                    if ((string) nameProp.GetMethod.Invoke(cd, null) == name)
                    {
                        return cd;
                    }
                }
                catch
                {
                }
            }


            var collectionGetters = props.Where(x => x.CanRead && typeof (IList).IsAssignableFrom(x.PropertyType)).ToArray();

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
