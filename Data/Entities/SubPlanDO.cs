using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Data.Entities
{
    public class SubplanDO : PlanNodeDO
    {
        /*
         * IMPORTANT: IF YOU'RE ADDING A NEW PROPERTY/FIELD, 
         * be sure to declare it in the following places, otherwise values 
         * of the new properties will not be persisted:
         * 
         * 1. Add it to the list of tracked properties, e.g. 
         *      typeof(SubplanDO).GetProperty(nameof(MyNewProperty))
         *    Note: don't add virtual navigation properties to this list, 
         *    only add the foreign key property for a navigation property. 
         * 
         * 2. Add it to the CopyProperties() method, e.g.
         *      MyNewProperty = plan.MyNewProperty;
         * 
         * 3. Add it to the SubPlan#Update method, e.g. 
         *      curSubPlan.Name = subplan.Name;
         *     
         */

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