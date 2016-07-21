using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class NodeTransitionDO : BaseObject
    {
        public int Id { get; set; }
        
        public PlanNodeTransitionType Transition { get; set; }

        /// <summary>
        /// Used if Containers generated from this PlanDescription would transition to another Activity described in this PlanDescription. Validation should make sure that all ActivityDescriptionIds can be found somewhere in the PlanDescription. This is jused for Jump to Activity and Jump to Subplan.
        /// </summary>
        public int? ActivityDescriptionId { get; set; }

        public ActivityDescriptionDO ActivityDescription { get; set; }

        /// <summary>
        /// Used if Containers generated from this PlanDescription would transition to Plans generated from another PlanDescription
        /// </summary>
        public int? PlanTemplateId { get; set; }

        public PlanTemplateDO PlanTemplate { get; set; }

        /// <summary>
        /// Used if Containers generated from this PlanDescription would transition to an existing Plan. Currently assumes that the existing Plan will be hosted by the same Hub. Eventually we'll need a way to address a Plan running on another Hub.
        /// </summary>
        public Guid? PlanId { get; set; }

        public PlanDO Plan { get; set; }
    }

    public enum PlanNodeTransitionType { Downstream, Child, Jump }
}
