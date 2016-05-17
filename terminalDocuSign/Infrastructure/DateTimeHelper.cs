using System;
using System.Globalization;

namespace terminalDocuSign.Infrastructure
{
    public static class DateTimeHelper
    {
        public static DateTime? Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            return DateTime.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}