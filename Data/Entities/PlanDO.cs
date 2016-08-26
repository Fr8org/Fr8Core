using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.States.Templates;
using System.Linq;
using System;
using System.Reflection;
using Data.States;
using Fr8.Infrastructure.Data.States;

namespace Data.Entities
{
    public class PlanDO : PlanNodeDO
    {
        /*
         * IMPORTANT: IF YOU'RE ADDING A NEW PROPERTY/FIELD, 
         * be sure to declare it in the following places, otherwise values 
         * of the new properties will not be persisted:
         * 
         * 1. Add it to the list of tracked properties, e.g. 
         *      typeof(PlanDO).GetProperty(nameof(MyNewProperty))
         *    Note: don't add virtual navigation properties to this list, 
         *    only add the foreign key property for a navigation property. 
         * 
         * 2. Add it to the CopyProperties() method, e.g.
         *      MyNewProperty = plan.MyNewProperty;
         *      
         * 3. Add it to the Plan#CreateOrUpdate method, e.g. 
         *      curPlan.MyNewProperty = submittedPlan.MyNewProperty;
         * 
         */

        private static readonly PropertyInfo[] TrackingProperties =
        {
            typeof(PlanDO).GetProperty(nameof(Name)),
            typeof(PlanDO).GetProperty(nameof(Tag)),
            typeof(PlanDO).GetProperty(nameof(Description)),
            typeof(PlanDO).GetProperty(nameof(PlanState)),
            typeof(PlanDO).GetProperty(nameof(Category)),
            typeof(PlanDO).GetProperty(nameof(Visibility)),
            typeof(PlanDO).GetProperty(nameof(IsApp)),
            typeof(PlanDO).GetProperty(nameof(AppLaunchURL))            
        };
     
        public PlanDO()
        {
            Visibility = PlanVisibility.Standard;
        }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        /*[ForeignKey("StartingSubPlan")]
        public int StartingSubPlanId { get; set; }

        public virtual SubPlanDO StartingSubPlan { get; set; }*/

        [NotMapped]
        public Guid StartingSubPlanId
        {
            get
            {
                var startingSubPlan = ChildNodes.OfType<SubplanDO>()
                    .SingleOrDefault(pnt => pnt.StartingSubPlan == true);
                if (null != startingSubPlan)
                {
                    return startingSubPlan.Id;
                }
                else
                {
                    return Guid.Empty;
                    //throw new ApplicationException("Starting SubPlan doesn't exist.");
                }
            }
        }

        [NotMapped]
        public SubplanDO StartingSubplan
        {
            get
            {
                return SubPlans.SingleOrDefault(pnt => pnt.StartingSubPlan == true);
            }

            set
            {
                var startingSubPlan = SubPlans.SingleOrDefault(pnt => pnt.StartingSubPlan == true);
                if (null != startingSubPlan)
                    startingSubPlan = value;
                else
                {
                    SubPlans.ToList().ForEach(pnt => pnt.StartingSubPlan = false);
                    if (value != null) 
                    { 
                        value.StartingSubPlan = true;
                        ChildNodes.Add(value);
                    }

                }
            }
        }

        [Required]
        [ForeignKey("PlanStateTemplate")]
        public int PlanState { get; set; }


        public virtual _PlanStateTemplate PlanStateTemplate { get; set; }

        public string Tag { get; set; }
        
        public PlanVisibility Visibility { get; set; }

        public bool IsApp { get; set; }

        public string AppLaunchURL { get; set; }

        public string Category { get; set; }

        [NotMapped]
        public IEnumerable<SubplanDO> SubPlans
        {
            get
            {
                return ChildNodes.OfType<SubplanDO>();
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

        protected override PlanNodeDO CreateNewInstance()
        {
            return new PlanDO();
        }

        protected override void CopyProperties(PlanNodeDO source)
        {
            var plan = (PlanDO)source;

            base.CopyProperties(source);
            Name = plan.Name;
            Tag = plan.Tag;
            PlanState = plan.PlanState;
            Description = plan.Description;
            Visibility = plan.Visibility;
            Category = plan.Category;
            LastUpdated = plan.LastUpdated;
            AppLaunchURL = plan.AppLaunchURL;
            IsApp = plan.IsApp;
        }
    }
}