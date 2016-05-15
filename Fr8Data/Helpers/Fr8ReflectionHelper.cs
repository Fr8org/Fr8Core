using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fr8Data.DataTransferObjects;

namespace Fr8Data.Helpers
{
    public class Fr8ReflectionHelper
    {
        public static IEnumerable<FieldDTO> FindFieldsRecursive(object obj)
        {
            var result = new List<FieldDTO>();
            if (obj == null)
            {
                return result;
            }
            var fieldDTO = obj as FieldDTO;
            if (fieldDTO != null)
            {
                result.Add(fieldDTO);
                return result;
            }
            var type = obj.GetType();
            if (IsPrimitiveType(type))
            {
                return result;
            }
            var list = obj as IEnumerable;
            if (list != null)
            {
                foreach (var element in list)
                {
                    result.AddRange(FindFieldsRecursive(element));
                }
                return result;
            }
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            //We skip indexer properties as we won't be able to supply arguments
            foreach (var property in properties.Where(x => x.GetIndexParameters().Length == 0))
            {
                if (IsPrimitiveType(property.PropertyType))
                {
                    result.Add(new FieldDTO(property.Name, property.GetValue(obj)?.ToString()));
                }
                else
                {
                    result.AddRange(FindFieldsRecursive(property.GetValue(obj)));
                }
            }
            foreach (var field in fields)
            {
                if (IsPrimitiveType(field.FieldType))
                {
                    result.Add(new FieldDTO(field.Name, field.GetValue(obj)?.ToString()));
                }
                else
                {
                    result.AddRange(FindFieldsRecursive(field.GetValue(obj)));
                }
            }
            return result;
        }

        public static bool CheckAttributeOrTrue<T>(IMemberAccessor memberAccessor, Predicate<T> predicate)
            where T : Attribute
        {
            var attribute = memberAccessor.GetCustomAttribute<T>();

            if (attribute == null)
            {
                return true;
            }

            return predicate(attribute);
        }

        public static object[] FindFirstArray(Object obj, int maxSearchDepth = 0)
        {
            return FindFirstArrayRecursive(obj, maxSearchDepth, 0);
        }

        public static object[] FindFirstArrayRecursive(Object obj, int maxSearchDepth, int depth)
        {
            if (maxSearchDepth != 0 && depth > maxSearchDepth || obj == null)
            {
                return null;
            }

            if (obj is IEnumerable)
            {
                return ((IEnumerable)obj).OfType<Object>().ToArray();
            }

            var objType = obj.GetType();
            bool isPrimitiveType = objType.IsPrimitive || objType.IsValueType || (objType == typeof(string));

            if (!isPrimitiveType)
            {
                var objProperties = objType.GetProperties();
                foreach (var prop in objProperties.Where(x => x.GetIndexParameters().Length == 0))
                {
                    var result = FindFirstArrayRecursive(prop.GetValue(obj), maxSearchDepth, depth + 1);

                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        public static IEnumerable<IMemberAccessor> GetMembers(Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Select(x => (IMemberAccessor)new PropertyMemberAccessor(x))
                       .Concat(type.GetFields(BindingFlags.Instance | BindingFlags.Public).Select(x => (IMemberAccessor)new FieldMemberAccessor(x)));
        }

        public static bool IsPrimitiveType(Type type)
        {
            return type.IsPrimitive
                   || type.IsValueType
                   || type == typeof(string)
                   || type == typeof(Guid)
                   || type == typeof(DateTime)
                   || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && IsPrimitiveType(type.GetGenericArguments()[0]));
        }
    }
}
