using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fr8Data.DataTransferObjects.PlanTemplates
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
