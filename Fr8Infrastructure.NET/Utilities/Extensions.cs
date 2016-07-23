using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Fr8.Infrastructure.Utilities
{
    public static class ObjectExtension
    {
        public static string to_S(this object value)
        {
            return value.ToString();
        }
    }

    public static class JsonExtensions
    {
        public static IEnumerable<JToken> WalkTokens(this JToken node)
        {
            if (node == null)
                yield break;
            yield return node;
            foreach (var child in node.Children())
                foreach (var childNode in child.WalkTokens())
                    yield return childNode;
        }
    }


    public static class StringExtension
    {
        public static string UppercaseFirst(this string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public static Uri AsUri(this string value)
        {
            return new Uri(value);
        }

        public static bool IsGuid(this string value)
        {
            Guid guid;
            return Guid.TryParse(value, out guid);
        }

        public static string StringCSharpLineBreakToHTMLLineBreak(this string value)
        {
            if (String.IsNullOrEmpty(value))
                return value;

            return value.Replace(Environment.NewLine, "<br/>");
        }

        public static short ToShort(this string value)
        {
            short returnValue;
            if (!short.TryParse(value, out returnValue))
            {
                throw new ArgumentException("invalid short number as parameter", "Parameter");
            }
            return returnValue;
        }
        public static int ToInt(this string value)
        {
            int returnValue;
            if (!int.TryParse(value, out returnValue))
            {
                throw new ArgumentException("invalid integer number as parameter", "Parameter");
            }
            return returnValue;
        }
        public static bool ToBool(this string value)
        {
            bool returnValue;
            if (IsNullOrEmpty(value))
                return false;

            if (!bool.TryParse(value, out returnValue))
            {
                throw new ArgumentException("invalid boolean as parameter", "Parameter");
            }
            return returnValue;
        }
        public static bool IsNullOrEmpty(this string value)
        {
            return String.IsNullOrEmpty(value) ||
                (!String.IsNullOrEmpty(value) && value.Trim() == String.Empty);
        }
        public static double ToDouble(this string value)
        {
            double returnValue;
            if (!double.TryParse(value, out returnValue))
            {
                throw new ArgumentException("invalid double number as parameter", "Parameter");
            }
            return returnValue;
        }

        public static bool EqualsIgnoreCase(this string left, string right)
        {
            return String.Compare(left, right, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        /// <summary>
        /// Uses Uri.EscapeDataString() based on recommendations on MSDN
        /// </summary>
        public static string UrlEncode(this string input)
        {
            return Uri.EscapeDataString(input);
        }

        public static string GetName(this Method method)
        {
            return Enum.GetName(typeof(Method), method);
        }
        public static string ToStr(this object value)
        {
            return Convert.ToString(value);
        }
        public static string format(this string msg, params object[] args)
        {
            return string.Format(msg, args);
        }
    }

    public static class UriExtensions
    {
        public static UriBuilder AddOrUpdateQueryParams(this UriBuilder uriBuilder, object parameters)
        {
            var paramValues = HttpUtility.ParseQueryString(uriBuilder.Query);
            foreach (var prop in parameters.GetType().GetProperties().Where(p => p.CanRead))
            {
                var value = prop.GetValue(parameters);
                if (value != null)
                {
                    paramValues.Set(prop.Name, value.ToString());
                }
            }
            uriBuilder.Query = paramValues.ToString();
            return uriBuilder;
        }

        public static Uri AddOrUpdateQueryParams(this Uri uri, object parameters)
        {
            var uriBuilder = new UriBuilder(uri);
            uriBuilder = uriBuilder.AddOrUpdateQueryParams(parameters);
            return uriBuilder.Uri;
        }
    }


    public static class DateTimeExtensions
    {
        /// <summary>
        /// Generates a Unix timestamp based on the current elapsed seconds since '01/01/1970 0000 GMT"
        /// </summary>
        /// <returns></returns>
        public static string ToUnixTime()
        {
            DateTime currentTime = DateTime.UtcNow;
            TimeSpan timeSpan = (currentTime - new DateTime(1970, 1, 1));
            string timestamp = timeSpan.TotalSeconds.ToString();

            return timestamp;
        }

        /// <summary>
        /// Current time in IS0 8601 format
        /// </summary>
        public static string ToIso8601Time()
        {
            DateTime currentTime = DateTime.UtcNow;
            string timestamp = currentTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
            return timestamp;
        }

        /// <summary>
        /// Return DateTime in UTC format
        /// </summary>
        public static DateTime GetUtcDateTime()
        {
            return DateTime.UtcNow;
        }
    }

    public static class DateTimeQuickValidateExtensions
    {
        public static string GenerateDateFromText(this string selected)
        {
            DateTime validDatetime;
            if (!DateTime.TryParse(selected, out validDatetime))
            {
                return "Invalid Selection";
            }
            return validDatetime.ToString("MM/dd/yyyy HH:mm");
        }
    }

    public static class EnumExtensions
    {
        public static string GetEnumDescription(this Enum value, string defaultValue = null)
        {
            return value.GetEnumAttribute<DescriptionAttribute>(a => a.Description, defaultValue);
        }
        public static string GetEnumDisplayName(this Enum value, string defaultValue = null)
        {
            return value.GetEnumAttribute<DisplayAttribute>(a => a.Name, defaultValue);
        }
        private static string GetEnumAttribute<TAttr>(this Enum value, Func<TAttr, string> expr, string defaultValue = null) where TAttr : Attribute
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            var attributes = fi.GetCustomAttributes<TAttr>(false).ToArray();
            return (attributes != null && attributes.Length > 0) ? expr(attributes.First()) : (defaultValue ?? value.ToString());
        }
    }


    public static class TypeExtensions
    {
        /// <summary>
        /// Retrieves all assemblies active in the current application
        /// WARNING: This will return EVERY assembly in your current domain, this is a costly operation and should only be used
        /// during initialization
        /// </summary>
        private static IList<Assembly> Environment
        {
            get { return AppDomain.CurrentDomain.GetAssemblies().ToList(); }
        }
        /// <summary>
        /// Retrieves just the Base.NET framework assembly
        /// </summary>
        private static Assembly Base
        {
            get { return Assembly.GetExecutingAssembly(); }
        }

        /// <summary>
        /// Returns all types INTERNALLY within the Base.NET assembly only.
        /// </summary>
        /// <typeparam name="TBase"></typeparam>
        /// <param name="includeBase">Includes the provided TBase type in the list</param>        
        /// <returns>A list of all types that implement the interface or inherit from the class</returns>
        public static Type[] GetInternalTypes<TBase>(params Type[] excludeTypes)
        {
            if (typeof(TBase).IsInterface)
            {
                return Base.GetTypes()
                           .Where(t => t.GetInterfaces().Contains(typeof(TBase)) && !excludeTypes.Contains(t))
                           .ToArray();
            }
            else {
                return Base.GetTypes()
                           .Where(t => typeof(TBase).IsAssignableFrom(t) && !excludeTypes.Contains(t) && t.IsClass && !t.IsAbstract)
                           .ToArray();
            }
        }
        /// <summary>
        /// Returns all types of TBase within the Base.NET assembly only and creates each search result as it's generic type instance
        /// </summary>
        /// <typeparam name="TBase">The base type to search for (all inherited class types of TBase will be matched)</typeparam>
        /// <param name="excludeTypes">Types to exclude from the results</param>
        /// <returns>A list of instances from the search results as type TBase</returns>
        public static TBase[] GetInternalTypesAsInstance<TBase>(params Type[] excludeTypes)
        {
            if (typeof(TBase).IsInterface)
            {
                return Base.GetTypes()
                                    .Where(t => t.GetInterfaces().Contains(typeof(TBase)) && !excludeTypes.Contains(t))
                                    .Select(t => (TBase)Activator.CreateInstance(t))
                                    .ToArray();
            }
            else {
                return Base.GetTypes()
                                    .Where(t => typeof(TBase).IsAssignableFrom(t) && !excludeTypes.Contains(t) && t.IsClass && !t.IsAbstract)
                                    .Select(t => (TBase)Activator.CreateInstance(t))
                                    .ToArray();
            }
        }

        /// <summary>
        /// Finds all types that have custom attributes of type TAttr within the Base.NET assembly only
        /// </summary>
        /// <typeparam name="TAttr">The attributes to find in which classes are utilizing</typeparam>
        /// <param name="inherit">Use inheritance in the search result</param>
        /// <param name="excludeTypes">Exclude types in this enumerable list</param>
        /// <returns>All types that contain the specified attribute</returns>
        public static Type[] GetInternalTypesWithAttribute<TAttr>(bool inherit = false, params Type[] excludeTypes)
        {
            return Base.GetTypes().Where(t => t.GetCustomAttributes(typeof(TAttr), inherit).Length > 0 && !excludeTypes.Contains(t)).ToArray();
        }

        /// <summary>
        /// Fetches all types from all assemblies in the current app domain
        /// NOTE: Requires a long search through all executing assemblies, can be very costly if used too often.
        /// </summary>
        /// <typeparam name="TBase">The base type class or interface from which types returned in the list inherit from or implement.</typeparam>
        /// <param name="excludeTypes">Exclude types in this enumerable list</param>
        /// <returns>An array of matched types</returns>
        public static Type[] GetTypes<TBase>(params Type[] excludeTypes)
        {
            List<Type> tlist = new List<Type>();
            foreach (Assembly a in Environment)
            {
                try
                {
                    if (typeof(TBase).IsInterface)
                    {
                        tlist.AddRange(a.GetTypes()
                                            .Where(t => t.GetInterfaces().Contains(typeof(TBase)) && !excludeTypes.Contains(t) && t.IsClass && !t.IsAbstract)
                                            .ToList());
                    }
                    else {
                        tlist.AddRange(a.GetTypes()
                                            .Where(t => typeof(TBase).IsAssignableFrom(t) && !excludeTypes.Contains(t) && t.IsClass && !t.IsAbstract)
                                            .ToArray());
                    }
                }
                catch (Exception)
                {
                    //TODO:: Log failed type reflections
                }
            }
            return tlist.ToArray();
        }

        /// <summary>
        /// Returns all types of TBase within the Base.NET assembly only and creates each search result as it's generic type instance
        /// </summary>
        /// <typeparam name="TBase">The base type to search for (all inherited class types of TBase will be matched)</typeparam>
        /// <param name="excludeTypes">Exclude types in this enumerable list</param>
        /// <returns>An array of instances that inherit or implement the matched type</returns>
        public static TBase[] GetTypesAsInstance<TBase>(params Type[] excludeTypes)
        {
            List<TBase> inst = new List<TBase>();
            foreach (Assembly a in Environment)
            {
                try
                {
                    if (typeof(TBase).IsInterface)
                    {
                        inst.AddRange(a.GetTypes()
                                       .Where(t => t.GetInterfaces().Contains(typeof(TBase)) && !excludeTypes.Contains(t))
                                       .Select(t => (TBase)Activator.CreateInstance(t))
                                       .ToList());
                    }
                    else {
                        inst.AddRange(a.GetTypes()
                                       .Where(t => typeof(TBase).IsAssignableFrom(t) && !excludeTypes.Contains(t) && t.IsClass && !t.IsAbstract)
                                       .Select(t => (TBase)Activator.CreateInstance(t))
                                       .ToArray());
                    }
                }
                catch (Exception)
                {
                    //TODO:: Log failed type reflections
                }
            }
            return inst.ToArray();
        }


        /// <summary>
        /// Retrieves all types in the current application (searches ALL assemblies) that contain the specified attribute type
        /// </summary>
        /// <typeparam name="TAttr">The type of attribute to match classes</typeparam>
        /// <param name="inherit">Use inheritance in the search</param>
        /// <param name="excludeTypes">Exclude these types from the search results</param>
        /// <returns>A list of types that contain the specified attribute type</returns>
        public static Type[] GetTypesWithAttribute<TAttr>(bool inherit = false, params Type[] excludeTypes)
        {
            List<Type> tlist = new List<Type>();
            foreach (Assembly a in Environment)
            {
                try
                {
                    tlist.AddRange(a.GetTypes().Where(t => t.GetCustomAttributes(typeof(TAttr), inherit).Length > 0 && !excludeTypes.Contains(t))
                                    .ToList());
                }
                catch (Exception)
                {
                    //TODO:: Log failed type reflections
                }
            }
            return tlist.ToArray();
        }

        public static Type[] GetBestGenericArgs(this Type t)
        {
            Type[] args = t.GetGenericArguments();
            Type[] cnst = args.SelectMany(x => x.GetGenericArguments()).ToArray();
            return (cnst.Length > 0) ? cnst.Take(cnst.Length)
                                           .Union(cnst.Skip(cnst.Length)
                                                      .Select(y => (t.ContainsGenericParameters) ? t.MakeGenericType(args.Select(z => typeof(object)).ToArray()) : t))
                                           .ToArray()
                                     : args.Select(x => (t.ContainsGenericParameters) ? t.MakeGenericType(args.Select(z => typeof(object)).ToArray()) : t)
                                           .ToArray();
        }

        public static bool IsActionDelegate(this Type t)
        {
            if (t.IsSubclassOf(typeof(MulticastDelegate)) &&
               t.GetMethod("Invoke").ReturnType == typeof(void))
                return true;
            return false;
        }
    }
}