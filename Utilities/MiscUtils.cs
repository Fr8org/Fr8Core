using System;
using System.Text;

namespace Utilities
{
    public static class MiscUtils
    {
        public static bool AreEqual(object firstValue, object secondValue)
        {
            return object.Equals(firstValue, secondValue);
        }

        /// <summary>
        /// The function masks password in a database connection string.
        /// </summary>
        /// <param name="cs"></param>
        /// <returns></returns>
        public static string MaskPassword(string cs)
        {
            var regex = new System.Text.RegularExpressions.Regex("password=([\\S^]+)[^;]", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            var match = regex.Match(cs);
            if (match == null || !match.Success || match.Groups.Count != 2)
            {
                return cs;
            }
            var group = match.Groups[1];
            return cs.Substring(0, group.Index) + "*****" + cs.Substring(group.Index + group.Length + 1);
        }

        public static StringBuilder Trim(this StringBuilder source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (source.Length == 0)
            {
                return source;
            }
            var index = source.Length - 1;
            while (index >= 0 && char.IsWhiteSpace(source[index]))
            {
                index--;
            }
            index = index == -1 ? 0 : index;
            source.Remove(index, source.Length - index);
            if (source.Length == 0)
            {
                return source;
            }
            index = 0;
            while (index < source.Length && char.IsWhiteSpace(source[index]))
            {
                index++;
            }
            return source.Remove(0, index);            
        }
    }
}
