using System.Collections.Generic;

namespace HubWeb.Infrastructure_PD.Interfaces
{
    public class SearchResultDTO
    {
        public IEnumerable<SearchItemDTO> PlanTemplates { get; set; }
        public long TotalCount { get; set; }
    }
}