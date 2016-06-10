using System;
using System.Collections.Generic;

namespace Fr8.Infrastructure.Data.DataTransferObjects.PlanTemplates
{
    public  class PlanTemplateDTO
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid? StartingPlanNodeDescriptionId { get; set; }

        public List<PlanNodeDescriptionDTO> PlanNodeDescriptions { get; set; }

        public string Description { get; set; }
    }
}
