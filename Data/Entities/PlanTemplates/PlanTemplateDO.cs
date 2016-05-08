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
    public class PlanTemplateDO : BaseObject
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        [ForeignKey("User")]
        public string Fr8AccountId { get; set; }
        [JsonIgnore]
        public Fr8AccountDO User { get; set; }

        [NotMapped]
        public int StartingPlanNodeDescriptionId
        {
            get
            {
                if (PlanNodeDescriptions != null)
                {
                    var startingSubPlan = PlanNodeDescriptions
                          .SingleOrDefault(pnt => pnt.IsStartingSubplan);
                    if (null != startingSubPlan)
                    {
                        return startingSubPlan.Id;
                    }
                }

                return 0;
            }
        }

        [NotMapped]
        public PlanNodeDescriptionDO StartingPlanNodeDescription
        {
            get
            {
                return PlanNodeDescriptions.SingleOrDefault(pnt => pnt.IsStartingSubplan == true);
            }

            set
            {
                var startingSubPlan = PlanNodeDescriptions.SingleOrDefault(pnt => pnt.IsStartingSubplan == true);
                if (null != startingSubPlan)
                    startingSubPlan = value;
                else
                {
                    PlanNodeDescriptions.ToList().ForEach(pnt => pnt.IsStartingSubplan = false);
                    if (value != null)
                    {
                        value.IsStartingSubplan = true;
                        PlanNodeDescriptions.Add(value);
                    }

                }
            }
        }

        public List<PlanNodeDescriptionDO> PlanNodeDescriptions { get; set; }
    }
}
