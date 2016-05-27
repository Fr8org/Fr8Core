using System.Collections.Generic;

namespace PlanDirectory.Interfaces
{
    public class SearchResultDTO
    {
        // TODO: FR-3539, fix this.
        // public IEnumerable<PublishPlanTemplateDTO> PlanTemplates { get; set; }
        public long TotalCount { get; set; }
    }
}