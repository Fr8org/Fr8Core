using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class PlanNodeDescriptionDO : BaseObject
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [ForeignKey("ParentNode")]
        public int? ParentNodeId { get; set; }
        public PlanNodeDescriptionDO ParentNode { get; set; }

        public List<ActivityTransitionDO> Transitions { get; set; }
        
        public ActivityDescriptionDO ActivityDescription { get; set; }

        public string SubPlanName { get; set; }

        public bool IsStartingSubplan { get; set; }
    }
}
