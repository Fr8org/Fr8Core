using System;
using Newtonsoft.Json.Linq;

namespace fr8.Infrastructure.Data.DataTransferObjects
{
    public class PublishPlanTemplateDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public JToken PlanContents { get; set; }
        public Guid ParentPlanId { get; set; }
    }
}