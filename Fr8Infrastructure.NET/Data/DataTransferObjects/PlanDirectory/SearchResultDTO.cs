using System.Collections.Generic;

namespace Fr8.Infrastructure.Data.DataTransferObjects.PlanDirectory
{
    public class SearchResultDTO
    {
        public IEnumerable<SearchItemDTO> PlanTemplates { get; set; }
        public long TotalCount { get; set; }
    }
}