using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Data.States;

namespace Data.Entities
{
    public class ActivityDO : PlanNodeDO
	{
        /*
         * IMPORTANT: IF YOU'RE ADDING A NEW PROPERTY/FIELD, 
         * be sure to declare it in the following places, otherwise values 
         * of the new properties will not be persisted:
         * 
         * 1. Add it to the list of tracked properties, e.g. 
         *      typeof(ActivityDO).GetProperty(nameof(MyNewProperty))
         *    Note: don't add virtual navigation properties to this list, 
         *    only add the foreign key property for a navigation property. 
         * 
         * 2. Add it to the CopyProperties() method, e.g.
         *      MyNewProperty = plan.MyNewProperty;
         *      
         * 3. Add it to the Activity#UpdateActivityProperties method, e.g. 
         *      existingActivity.MyNewProperty = submittedActivity.MyNewProperty;
         * 
         */

        //Yes, we don't want to add this property to TrackingProperties or clone its value in CopyProperties.
        public byte[] EncryptedCrateStorage { get; set; }

        public string CrateStorage { get; set; }

        public string Label { get; set; }

        [ForeignKey("ActivityTemplate")]
        public Guid ActivityTemplateId { get; set; }

        public virtual ActivityTemplateDO ActivityTemplate { get; set; }

        [ForeignKey("AuthorizationToken")]
        public Guid? AuthorizationTokenId { get; set; }

        public virtual AuthorizationTokenDO AuthorizationToken { get; set; }

        public ActivationState ActivationState { get; set; }

        protected override PlanNodeDO CreateNewInstance()
        {
            return new ActivityDO();
        }


        private static readonly PropertyInfo[] TrackingProperties = 
        {
            typeof(ActivityDO).GetProperty(nameof(CrateStorage)),
            typeof(ActivityDO).GetProperty(nameof(Label)),
            typeof(ActivityDO).GetProperty(nameof(ActivityTemplateId)),
            typeof(ActivityDO).GetProperty(nameof(AuthorizationTokenId)),
            typeof(ActivityDO).GetProperty(nameof(ActivationState)),
        };

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

        protected override void CopyProperties(PlanNodeDO source)
        {
            var activity = (ActivityDO) source;

            base.CopyProperties(source);
            Label = activity.Label;
            CrateStorage = activity.CrateStorage;
            AuthorizationTokenId = activity.AuthorizationTokenId;
            ActivityTemplateId = activity.ActivityTemplateId;
            ActivationState = activity.ActivationState;
        }
    }
}