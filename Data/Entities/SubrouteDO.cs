using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Data.Entities
{
    public class SubrouteDO : RouteNodeDO
    {
        private static readonly PropertyInfo[] TrackingProperties =
        {
            typeof (SubrouteDO).GetProperty("Name"),
            typeof (SubrouteDO).GetProperty("StartingSubroute"),
            typeof (SubrouteDO).GetProperty("NodeTransitions"),
        };

        public SubrouteDO(bool startingSubroute)
        {
            StartingSubroute = startingSubroute;
        }

        public SubrouteDO()
            : this(false)
        {
        }

        public string Name { get; set; }

        public bool StartingSubroute { get; set; } 

        /// <summary>
        /// this is a JSON structure that is a array of key-value pairs that represent possible transitions. Example:
        ///[{'TransitionKey':'true','ProcessNodeId':'234kljdf'},{'TransitionKey':'false','ProcessNodeId':'dfgkjfg'}]. In this case the values are Id's of other ProcessNodes.
        /// </summary>
        public string NodeTransitions { get; set; }
        
        [NotMapped]
        public PlanDO Plan
        {
            get { return (PlanDO) ParentRouteNode; }
        }

        public override string ToString()
        {
            return this.Name;
        }

        public override void BeforeSave()
        {
            base.BeforeSave();

            // TODO: commented out.
            // TODO: Currently crashes on Route creation.
            //       When Route is created, empty StartSubroute is created and assigned to Route.
            //       Need to create another issue to fix that.
            // SubroutetValidator curValidator = new SubroutetValidator();
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

        protected override RouteNodeDO CreateNewInstance()
        {
            return new SubrouteDO();
        }

        protected override void CopyProperties(RouteNodeDO source)
        {
            var subroute = (SubrouteDO)source;

            base.CopyProperties(source);
            Name = subroute.Name;
            StartingSubroute = subroute.StartingSubroute;
            NodeTransitions = subroute.NodeTransitions;
        }
    }
}