using System;
using Newtonsoft.Json.Linq;

namespace Fr8.TerminalBase.Infrastructure
{
    /// <summary>
    /// JSON utility class, to help parse JSON data in terminals.
    /// </summary>
    public static class JsonParser
    {
        /// <summary>
        /// Extract property value from JSON object.
        /// </summary>
        public static T ExtractPropertyValue<T>(this JObject obj, string propertyName)
        {
            var valueToken = obj.GetValue(propertyName);
            if (valueToken == null
                || valueToken.ToObject<T>() == null)
            {
                throw new Exception(string.Format("\"{0}\" attribute is not specified", propertyName));
            }

            var result = valueToken.ToObject<T>();
            return result;
        }
    }
}
