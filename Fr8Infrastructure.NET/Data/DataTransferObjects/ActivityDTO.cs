using System;
using Fr8.Infrastructure.Data.Convertors.JsonNet;
using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class ActivityDTO 
    {
        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("activityTemplate")]
        public ActivityTemplateSummaryDTO ActivityTemplate { get; set; }

        [JsonProperty("planId")]
        public Guid? RootPlanNodeId { get; set; }

        [JsonProperty("parentPlanNodeId")]
        public Guid? ParentPlanNodeId { get; set; }

        [JsonProperty("ordering")]
        public int Ordering { get; set; }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("crateStorage")]
        public CrateStorageDTO CrateStorage { get; set; }

        [JsonProperty("childrenActivities")]
        public ActivityDTO[] ChildrenActivities { get; set; }

        [JsonProperty("authTokenId")]
        public Guid? AuthTokenId { get; set; }

        [JsonProperty("authToken")]
        public AuthorizationTokenDTO AuthToken { get; set; }

        [JsonProperty("documentation")]
        public string Documentation { get; set; }

    }
}
