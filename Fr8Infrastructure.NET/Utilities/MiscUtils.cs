using System;
using System.Text;

namespace Fr8.Infrastructure.Utilities
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
            var index = source.Length;
            while (index > 0 && char.IsWhiteSpace(source[index - 1]))
            {
                index--;
            }
            if (index != source.Length)
            {
                source.Remove(index, source.Length - index);
            }
            if (source.Length == 0)
            {
                return source;
            }
            var trimLength = 0;
            while (trimLength < source.Length && char.IsWhiteSpace(source[trimLength]))
            {
                trimLength++;
            }
            return source.Remove(0, trimLength);            
        }

        /// <summary>
        /// Given a directory path, returns an upper level path by the specified number of levels up.
        /// </summary>
        public static string UpNLevels(string path, int levels)
        {
            int index = path.LastIndexOf('\\', path.Length - 1, path.Length);
            if (index <= 3)
                return string.Empty;
            string result = path.Substring(0, index);
            if (levels > 1)
            {
                result = UpNLevels(result, levels - 1);
            }
            return result;
        }

    }
}
