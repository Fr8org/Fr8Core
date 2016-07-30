using System;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class PublishPlanTemplateDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public PlanFullDTO PlanContents { get; set; }
        public Guid ParentPlanId { get; set; }
    }
}