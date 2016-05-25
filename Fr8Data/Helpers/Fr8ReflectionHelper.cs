using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;

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


            var propsToIgnore = new HashSet<string>();
            var manifestType = typeof(Manifest);
            var members = Fr8ReflectionHelper.GetMembers(type);

            // ingore properties from Manifest base class
            if (manifestType.IsAssignableFrom(type))
            {
                foreach (var prop in Fr8ReflectionHelper.GetMembers(manifestType))
                {
                    propsToIgnore.Add(prop.Name);
                }
            }

            foreach (var memberAccessor in members)
            {
                if (propsToIgnore.Contains(memberAccessor.Name))
                {
                    continue;
                }

                if (IsPrimitiveType(memberAccessor.MemberType))
                {
                    result.Add(new FieldDTO(memberAccessor.Name, memberAccessor.GetValue(obj)?.ToString()));
                }
                else
                {
                    result.AddRange(FindFieldsRecursive(memberAccessor.GetValue(obj)));
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
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => x.GetIndexParameters().Length == 0).Select(x => (IMemberAccessor)new PropertyMemberAccessor(x))
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

        public static bool CheckIfMemberIsCollectionOf<TItem>(IMemberAccessor member)
        {
            if (member.MemberType.IsInterface && CheckIfTypeIsCollectionOf<TItem>(member.MemberType))
            {
                return true;
            }

            return member.MemberType.GetInterfaces().Any(x => CheckIfTypeIsCollectionOf(x, typeof(TItem)));
        }

        public static bool CheckIfTypeIsCollectionOf<TItem>(Type type)
        {
            return CheckIfTypeIsCollectionOf(type, typeof(TItem));
        }

        public static bool CheckIfTypeIsCollectionOf(Type type, Type itemType)
        {
            var enumerableInterface = type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>) ? type : type.GetInterface("IEnumerable`1");
            if (enumerableInterface == null)
            {
                return false;
            }
            return itemType.IsAssignableFrom(enumerableInterface.GetGenericArguments()[0]);
        }
    }
}
