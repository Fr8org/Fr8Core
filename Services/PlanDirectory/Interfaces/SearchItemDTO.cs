using System;

namespace PlanDirectory.Interfaces
{
    public class SearchItemDTO
    {
        public Guid ParentPlanId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}