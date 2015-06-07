using System;

namespace Utilities
{
    public static class ObjectExtension
    {
        public static string to_S(this object value)
        {
            return value.ToString();
        }
    }



    public static class StringExtension
    {
        public static Uri AsUri(this string value)
        {
            return new Uri(value);
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
            if (!DateTime.TryParse(selected, out validDatetime)) {
                return "Invalid Selection";
            }
            return validDatetime.ToString("MM/dd/yyyy HH:mm");
        }
    }
}