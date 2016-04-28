using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces.DataTransferObjects.PlanDescription
{
    public  class PlanDescriptionDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? StartingPlanNodeDescriptionId { get; set; }

        public List<PlanNodeDescriptionDTO> PlanNodeDescriptions { get; set; }

        public string Description { get; set; }
    }
}
