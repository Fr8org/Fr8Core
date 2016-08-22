using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Data.Entities
{
    public class SubplanDO : PlanNodeDO
    {
        private static readonly PropertyInfo[] TrackingProperties =
        {
            typeof (SubplanDO).GetProperty(nameof(Name)),
            typeof (SubplanDO).GetProperty(nameof(StartingSubPlan)),
        };

        public SubplanDO(bool startingSubPlan)
        {
            StartingSubPlan = startingSubPlan;
        }

        public SubplanDO()
            : this(false)
        {
        }

        public string Name { get; set; }

        public bool StartingSubPlan { get; set; }

        [NotMapped]
        public PlanDO Plan
        {
            get { return (PlanDO)ParentPlanNode; }
        }

        public override string ToString()
        {
            return this.Name;
        }
        
        protected override IEnumerable<PropertyInfo> GetTrackingProperties()
        {
            foreach (var trackingProperty in base.GetTrackingProperties())
            {
                yield return trackingProperty;
            }

            foreach (var trackingProperty in TrackingProperties)
            {
                yield return trackingProperty;
            }
        }

        protected override PlanNodeDO CreateNewInstance()
        {
            return new SubplanDO();
        }

        protected override void CopyProperties(PlanNodeDO source)
        {
            var subPlan = (SubplanDO)source;

            base.CopyProperties(source);
            Name = subPlan.Name;
            StartingSubPlan = subPlan.StartingSubPlan;
        }
    }
}