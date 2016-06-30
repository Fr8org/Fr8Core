using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Fr8.Infrastructure.Utilities
{
    public static class StopwordsRemover
    {
        public static string RemoveStopwords(this string source, IEnumerable<string> stopwords)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }
            if (stopwords == null)
            {
                return source;
            }
            //Just to make sure we deal with fixed collection
            stopwords = stopwords as IList<string> ?? stopwords.ToArray();
            if (!stopwords.Any())
            {
                return source;
            }
            //TODO: if the need arises add aditional processing to escape special regex-related symbols inside stopword
            var regex = new Regex(string.Join("|", stopwords.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => $@"(^{x}\W{{1}})|(\W{{1}}{x}\W{{1}})|(\W{{1}}{x}$)")), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            var matches = regex.Matches(source);
            if (matches.Count == 0)
            {
                return source;
            }
            var result = new StringBuilder(source);
            //Sort matches in reversed order to remove them without spanning multiple garabage string objects
            foreach (var match in matches.Cast<Match>().OrderByDescending(x => x.Index))
            {
                var removeRange = FindRemoveRange(match);
                result.Remove(removeRange.Key, removeRange.Value - removeRange.Key + 1);                
            }
            result.Trim().Replace("  ", " ");
            return result.ToString();
        }

        private static KeyValuePair<int, int> FindRemoveRange(Match match)
        {
            var from = 0;
            var to = match.Length - 1;
            var startsWithSeparator = !char.IsLetterOrDigit(match.Value[from]) && match.Value[from] != '_';
            var endsWithSeparator = !char.IsLetterOrDigit(match.Value[to]) && match.Value[to] != '_';
            if (startsWithSeparator && endsWithSeparator)
            {
                //Stopword is in the middle of string - we need to adjust remove position
                var startsWithSpace = char.IsWhiteSpace(match.Value[from]);
                var endsWithSpace = char.IsWhiteSpace(match.Value[to]);
                if ((startsWithSpace && endsWithSpace) || (!startsWithSpace && !startsWithSpace))
                {
                    //Remove only the last separator
                    from++;
                }
                else if (startsWithSpace)
                {
                    //Remove nonspace separator from the end e.g 'Sam-' from 'The Sam-Co company'
                    from++;
                }
                else if (endsWithSpace)
                {
                    //Remove nonspace separator from the start e.g. '-Co' from 'The Sam-Co company'
                    to--;
                }
            }
            //Remove the full match value
            return new KeyValuePair<int, int>(match.Index + from, match.Index + to);
        }

        public static string RemoveStopwords(this string source, params string[] stopwords)
        {
            return RemoveStopwords(source, stopwords as IEnumerable<string>);
        }
    }
}
