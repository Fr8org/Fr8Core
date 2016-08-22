using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fr8Data.DataTransferObjects.PlanTemplates
{
    public class NodeTransitionDTO
    {
        public Guid Id { get; set; }

        public string Transition { get; set; }

        /// <summary>
        /// Used if Containers generated from this PlanDescription would transition to another Activity described in this PlanDescription. Validation should make sure that all ActivityDescriptionIds can be found somewhere in the PlanDescription. This is jused for Jump to Activity and Jump to Subplan.
        /// </summary>
        public Guid? ActivityDescriptionId { get; set; }

        /// <summary>
        /// Used if Containers generated from this PlanDescription would transition to Plans generated from another PlanDescription
        /// </summary>
        // public Guid? PlanTemplateId { get; set; }

        /// <summary>
        /// Used if Containers generated from this PlanDescription would transition to an existing Plan. Currently assumes that the existing Plan will be hosted by the same Hub. Eventually we'll need a way to address a Plan running on another Hub.
        /// </summary>
        public Guid? PlanId { get; set; }
    }
}
