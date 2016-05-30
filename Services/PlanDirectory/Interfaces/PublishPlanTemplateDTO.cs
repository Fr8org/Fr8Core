using System;
using Newtonsoft.Json.Linq;

namespace PlanDirectory.Interfaces
{
    public class PublishPlanTemplateDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public JToken PlanContents { get; set; }
        public Guid ParentPlanId { get; set; }
    }
}