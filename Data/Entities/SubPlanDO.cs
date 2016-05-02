using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Data.Entities
{
    public class SubPlanDO : PlanNodeDO
    {
        private static readonly PropertyInfo[] TrackingProperties =
        {
            typeof (SubPlanDO).GetProperty(nameof(Name)),
            typeof (SubPlanDO).GetProperty(nameof(StartingSubPlan)),
            typeof (SubPlanDO).GetProperty(nameof(NodeTransitions)),
        };

        public SubPlanDO(bool startingSubPlan)
        {
            StartingSubPlan = startingSubPlan;
        }

        public SubPlanDO()
            : this(false)
        {
        }

        public string Name { get; set; }

        public bool StartingSubPlan { get; set; }

        /// <summary>
        /// this is a JSON structure that is a array of key-value pairs that represent possible transitions. Example:
        ///[{'TransitionKey':'true','ProcessNodeId':'234kljdf'},{'TransitionKey':'false','ProcessNodeId':'dfgkjfg'}]. In this case the values are Id's of other ProcessNodes.
        /// </summary>
        public string NodeTransitions { get; set; }

        [NotMapped]
        public PlanDO Plan
        {
            get { return (PlanDO)ParentPlanNode; }
        }

        public override string ToString()
        {
            return this.Name;
        }

        public override void BeforeSave()
        {
            base.BeforeSave();

            // TODO: commented out.
            // TODO: Currently crashes on Plan creation.
            //       When Plan is created, empty StartSubPlan is created and assigned to Plan.
            //       Need to create another issue to fix that.
            // SubPlantValidator curValidator = new SubPlantValidator();
            // curValidator.ValidateAndThrow(this);
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
            return new SubPlanDO();
        }

        protected override void CopyProperties(PlanNodeDO source)
        {
            var subPlan = (SubPlanDO)source;

            base.CopyProperties(source);
            Name = subPlan.Name;
            StartingSubPlan = subPlan.StartingSubPlan;
            NodeTransitions = subPlan.NodeTransitions;
        }
    }
}