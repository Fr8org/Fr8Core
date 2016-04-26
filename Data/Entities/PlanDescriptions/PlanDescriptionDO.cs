using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class PlanDescriptionDO : BaseObject
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? StartingPlanNodeDescriptionId { get; set; }

        [ForeignKey("User")]
        public string Fr8AccountId { get; set; }
        [JsonIgnore]
        public Fr8AccountDO User { get; set; }

        public PlanNodeDescriptionDO StartingPlanNodeDescription { get; set; }

        public List<PlanNodeDescriptionDO> PlanNodeDescriptions { get; set; }
    }
}
