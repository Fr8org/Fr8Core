using Data.States;
using Data.States.Templates;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoMapper;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace Data.Entities
{
    public class ActivityDO : RouteNodeDO
	{
        public string Name { get; set; }

        public string CrateStorage { get; set; }
        public string Label { get; set; }

        [ForeignKey("ActivityTemplate")]
        public int? ActivityTemplateId { get; set; }

        public virtual ActivityTemplateDO ActivityTemplate { get; set; }

        [NotMapped]
        public bool IsTempId { get; set; }

        [NotMapped]
        public string ExplicitData { get; set; }

        public string currentView { get; set; }
        
        [ForeignKey("AuthorizationToken")]
        public Guid? AuthorizationTokenId { get; set; }

        public virtual AuthorizationTokenDO AuthorizationToken { get; set; }

        public string GetLoopId()
        {
            return Id.ToString();
        }

        public override bool AreContentPropertiesEquals(RouteNodeDO other)
        {
            var activity = (ActivityDO)other;
            
            return base.AreContentPropertiesEquals(other) &&
                   Name == activity.Name &&
                   Label == activity.Label &&
                   AuthorizationTokenId == activity.AuthorizationTokenId &&
                   CrateStorage == activity.CrateStorage &&
                   ActivityTemplateId == activity.ActivityTemplateId &&
                   currentView == activity.currentView &&
                   ExplicitData == activity.ExplicitData;
        }

        protected override RouteNodeDO CreateNewInstance()
        {
            return new ActivityDO();
        }


        private static readonly PropertyInfo[] TrackingProperties = 
        {
            typeof(ActivityDO).GetProperty("Name"),
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

        protected override void CopyProperties(RouteNodeDO source)
        {
            var activity = (ActivityDO) source;

            base.CopyProperties(source);
            Name = activity.Name;
            Label = activity.Label;
            CrateStorage = activity.CrateStorage;
            AuthorizationTokenId = activity.AuthorizationTokenId;
            ActivityTemplateId = activity.ActivityTemplateId;
            currentView = activity.currentView;
            ExplicitData = activity.ExplicitData;
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