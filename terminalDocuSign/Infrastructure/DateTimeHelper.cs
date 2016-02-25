using System;
using System.Globalization;

namespace terminalDocuSign.Infrastructure
{
    public static class DateTimeHelper
    {
        public static DateTime? Parse(string value)
        {
            if (value == null)
            {
                return null;
            }

            return DateTime.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}