using System.Collections.Generic;

namespace KwasantICS.Collections.Interfaces.Proxies
{
    public interface IGroupedValueListProxy<TItem, TValue> :
        IList<TValue>
    {
        IEnumerable<TItem> Items { get; }
    }
}
