using System;
using Newtonsoft.Json;
using Data.Infrastructure.JsonNet;

namespace Data.Interfaces.DataTransferObjects
{
    public class ActivityDTO 
    {
        public string Label { get; set; }

        [JsonProperty("activityTemplate")]
        [JsonConverter(typeof(ActivityTemplateActivityConverter))]
        public ActivityTemplateDTO ActivityTemplate { get; set; }

        public Guid? RootRouteNodeId { get; set; }

        public Guid? ParentRouteNodeId { get; set; }

        public string CurrentView { get; set; }

        public int Ordering { get; set; }

        public Guid Id { get; set; }

        public CrateStorageDTO CrateStorage { get; set; }

        public ActivityDTO[] ChildrenActions { get; set; }

        public AuthorizationTokenDTO AuthToken { get; set; }

        public int? ActivityTemplateId { get; set; }

        public bool IsTempId { get; set; }

        [JsonIgnore]
        public bool IsExplicitData { get; set; }

        //[JsonIgnore]
        public string ExplicitData { get; set; }

        [JsonIgnore]
        public Guid ContainerId { get; set; }

        [JsonIgnore]
        public string Fr8AccountId { get; set; }
                
        [JsonProperty("DocumentationSupport")]
        public string DocumentationSupport { get; set; }

    }
}
