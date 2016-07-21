using System.Collections.Generic;

namespace PlanDirectory.Interfaces
{
    public class SearchResultDTO
    {
        public IEnumerable<SearchItemDTO> PlanTemplates { get; set; }
        public long TotalCount { get; set; }
    }
}