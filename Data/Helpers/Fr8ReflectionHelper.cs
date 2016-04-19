using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Data.Interfaces.DataTransferObjects;

namespace Data.Helpers
{
    public class Fr8ReflectionHelper
    {
        public static IEnumerable<FieldDTO> FindFieldsRecursive(Object obj)
        {
            var fields = new List<FieldDTO>();
            if (obj == null)
            {
                return fields;
            }
            if (obj is IEnumerable && !(obj is string))
            {

                var objList = obj as IEnumerable;
                foreach (var element in objList)
                {
                    fields.AddRange(FindFieldsRecursive(element));
                }
                return fields;
            }

            var objType = obj.GetType();
            bool isPrimitiveType = objType.IsPrimitive || objType.IsValueType || (objType == typeof(string));

            if (!isPrimitiveType)
            {
                var field = obj as FieldDTO;
                if (field != null)
                {
                    return new List<FieldDTO> { field };
                }

                var objProperties = objType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public| BindingFlags.Instance | BindingFlags.Static);
                var objFields = objType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                foreach (var prop in objProperties)
                {
                    fields.AddRange(FindFieldsRecursive(prop.GetValue(obj)));
                }

                foreach (var prop in objFields)
                {
                    fields.AddRange(FindFieldsRecursive(prop.GetValue(obj)));
                }
            }

            return fields;
        }

        public static bool CheckAttributeOrTrue<T>(IMemberAccessor memberAccessor, Predicate<T> predicate)
            where T:Attribute
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
                foreach (var prop in objProperties)
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
                   || type == typeof (string)
                   || type == typeof (Guid)
                   || type == typeof (DateTime)
                   || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>) && IsPrimitiveType(type.GetGenericArguments()[0]));
        }
    }
}
