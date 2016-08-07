using System;

namespace Fr8.Infrastructure.Data.DataTransferObjects.PlanDirectory
{
    public class SearchItemDTO
    {
        public Guid ParentPlanId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Owner { get; set; }
    }
}