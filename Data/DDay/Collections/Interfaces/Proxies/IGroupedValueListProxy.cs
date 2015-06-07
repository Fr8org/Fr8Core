using System.Collections.Generic;

namespace Data.DDay.Collections.Interfaces.Proxies
{
    public interface IGroupedValueListProxy<TItem, TValue> :
        IList<TValue>
    {
        IEnumerable<TItem> Items { get; }
    }
}
