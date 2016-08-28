using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace terminalSalesforce.Infrastructure
{
    public interface ISalesforceFilterBuilder
    {
        string BuildFilter(IList<FieldDTO> fields, ICollection<FilterConditionDTO> conditions);
    }
}
