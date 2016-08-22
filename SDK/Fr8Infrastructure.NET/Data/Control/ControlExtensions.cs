using System.Collections.Generic;
using System.Linq;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Data.Control
{
    public static class DataExtensions
    {
        public static IEnumerable<ListItem> ToListItems(this IEnumerable<KeyValueDTO> fields)
        {
            return fields.Select(x => new ListItem() { Key = x.Key, Value = x.Value });
        }
    }
}
