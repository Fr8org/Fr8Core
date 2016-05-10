using Data.Interfaces.DataTransferObjects.PlanTemplates;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fr8Data.DataTransferObjects.PlanTemplates
{
    public class PlanNodeDescriptionDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int ParentNodeId { get; set; }

        public List<NodeTransitionDTO> Transitions { get; set; }

        public ActivityDescriptionDTO ActivityDescription { get; set; }

        public string SubPlanName { get; set; }

        public string SubPlanOriginalId { get; set; }

        public bool IsStartingSubplan { get; set; }
    }
}
