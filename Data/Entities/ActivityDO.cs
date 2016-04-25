using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Data.Entities
{
    public class ActivityDO : PlanNodeDO
	{
        public string CrateStorage { get; set; }
        public string Label { get; set; }

        [ForeignKey("ActivityTemplate")]
        public Guid ActivityTemplateId { get; set; }

        public virtual ActivityTemplateDO ActivityTemplate { get; set; }
        public string currentView { get; set; }

        [ForeignKey("AuthorizationToken")]
        public Guid? AuthorizationTokenId { get; set; }

        public virtual AuthorizationTokenDO AuthorizationToken { get; set; }

        protected override PlanNodeDO CreateNewInstance()
        {
            return new ActivityDO();
        }


        private static readonly PropertyInfo[] TrackingProperties = 
        {
            typeof(ActivityDO).GetProperty("CrateStorage"),
            typeof(ActivityDO).GetProperty("Label"),
            typeof(ActivityDO).GetProperty("ActivityTemplateId"),
            typeof(ActivityDO).GetProperty("AuthorizationTokenId"),
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
            currentView = activity.currentView;
        }

        //        public CrateStorageDTO CrateStorageDTO()
        //        {
        //            return JsonConvert.DeserializeObject<CrateStorageDTO>(this.CrateStorage);
        //        }
        //
        //        public void UpdateCrateStorageDTO(List<CrateDTO> curCratesDTO)
        //        {
        //            CrateStorageDTO crateStorageDTO = new CrateStorageDTO();
        //
        //            if(!String.IsNullOrEmpty(CrateStorage))//if crateStorage is not empty deserialize it
        //                crateStorageDTO = CrateStorageDTO();
        //
        //            crateStorageDTO.CrateDTO.AddRange(curCratesDTO);
        //
        //            this.CrateStorage = JsonConvert.SerializeObject(crateStorageDTO);
        //        }
    }
}