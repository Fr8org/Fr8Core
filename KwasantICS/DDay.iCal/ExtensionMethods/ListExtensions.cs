using System.Collections.Generic;

namespace KwasantICS.DDay.iCal.ExtensionMethods
{
    public static class ListExtensions
    {
        static public void AddRange<T>(this IList<T> list, IEnumerable<T> values)
        {
            if (values != null)
            {
                foreach (T item in values)
                    list.Add(item);
            }
        }
    }
}
