using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Data.Utility
{

    // This class can be used to copy properties from one instance to another. This methods is no recursive
    public static class CopyPropertiesHelper
    {
        private static readonly Dictionary<Type, Action<object, object, bool, Predicate<PropertyInfo>>> FieldsMap = new Dictionary<Type, Action<object, object, bool, Predicate<PropertyInfo>>>();

        //sometimes we don't want to copy virtual properties -> !x.GetGetMethod().IsVirtual because in EF they are navigational properties 
        // and their values must be handled in some special way that is out of the scope of this helper.
        public static void CopyProperties<T>(T source, T target, bool includeVirtual, Predicate<PropertyInfo> propertiesFilter = null)
        {
            Action<object, object, bool, Predicate<PropertyInfo>> copyFieldsAction;

            lock (FieldsMap)
            {
                var type = typeof (T);
                
                if (!FieldsMap.TryGetValue(type, out copyFieldsAction))
                {
                    // we skip indexers -> x.GetIndexParameters().Length == 0
                    var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => x.CanRead && x.CanWrite && x.GetIndexParameters().Length == 0).Select(x => new
                    {
                        Property = x,
                        IsVirtual = x.GetGetMethod().IsVirtual && !x.GetGetMethod().IsFinal
                    }).ToArray();
                    copyFieldsAction = (src, tgt, virt, filter) =>
                    {
                        foreach (var propertyInfo in props)
                        {
                            if ((virt || !propertyInfo.IsVirtual) && (filter == null || filter(propertyInfo.Property)))
                            {
                                try
                                {
                                    propertyInfo.Property.SetValue(tgt, propertyInfo.Property.GetValue(src));
                                }
                                catch (Exception ex)
                                {
                                    throw new InvalidOperationException(string.Format("Unable to copy property: {0}", propertyInfo.Property.Name), ex);
                                }
                            }
                        }
                    };

                    FieldsMap[type] = copyFieldsAction;
                }
            }

            copyFieldsAction(source, target, includeVirtual, propertiesFilter);
        }
    }
}
