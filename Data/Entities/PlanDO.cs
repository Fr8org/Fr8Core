using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.States.Templates;
using System.Linq;
using System;
using System.Reflection;

namespace Data.Entities
{
    public class PlanDO : RouteNodeDO
    {
        private static readonly PropertyInfo[] TrackingProperties =
        {
            typeof(PlanDO).GetProperty("Name"),
            typeof(PlanDO).GetProperty("Tag"),
            typeof(PlanDO).GetProperty("Description"),
            typeof(PlanDO).GetProperty("RouteState"),
            typeof(PlanDO).GetProperty("Category")
        };
     
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        /*[ForeignKey("StartingSubroute")]
        public int StartingSubrouteId { get; set; }

        public virtual SubrouteDO StartingSubroute { get; set; }*/

        [NotMapped]
        public Guid StartingSubrouteId
        {
            get
            {
                var startingSubroute = ChildNodes.OfType<SubrouteDO>()
                    .SingleOrDefault(pnt => pnt.StartingSubroute == true);
                if (null != startingSubroute)
                {
                    return startingSubroute.Id;
                }
                else
                {
                    return Guid.Empty;
                    //throw new ApplicationException("Starting Subroute doesn't exist.");
                }
            }
        }

        [NotMapped]
        public SubrouteDO StartingSubroute
        {
            get
            {
                return Subroutes.SingleOrDefault(pnt => pnt.StartingSubroute == true);
            }

            set {
                var startingSubroute = Subroutes.SingleOrDefault(pnt => pnt.StartingSubroute == true);
                if (null != startingSubroute)
                    startingSubroute = value;
                else
                {
                    Subroutes.ToList().ForEach(pnt => pnt.StartingSubroute = false);
                    if (value != null) 
                    { 
                        value.StartingSubroute = true;
                        ChildNodes.Add(value);
                    }

                }
            }
        }

        [Required]
        [ForeignKey("RouteStateTemplate")]
        public int RouteState { get; set; }

        public virtual _RouteStateTemplate RouteStateTemplate { get; set; }

        public string Tag { get; set; }

        public string Category { get; set; }

        [NotMapped]
        public IEnumerable<SubrouteDO> Subroutes
        {
            get
            {
                return ChildNodes.OfType<SubrouteDO>();
            }
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
            return new PlanDO();
        }


        protected override void CopyProperties(RouteNodeDO source)
        {
            var plan = (PlanDO)source;

            base.CopyProperties(source);
            Name = plan.Name;
            Tag = plan.Tag;
            RouteState = plan.RouteState;
            Description = plan.Description;
            Category = plan.Category;
        }
    }
}