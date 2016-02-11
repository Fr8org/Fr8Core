using System;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class ActivityDTO : ActivityDTOBase
    {
        public Guid? RootRouteNodeId { get; set; }

        public Guid? ParentRouteNodeId { get; set; }

        public CrateStorageDTO CrateStorage { get; set; }

        public string Label { get; set; }

        [JsonProperty("activityTemplate")]
        public ActivityTemplateDTO ActivityTemplate { get; set; }

        [JsonProperty("action_version")]
        public string ActivityVersion { get; set; }

        public string CurrentView { get; set; }

        public AuthorizationTokenDTO AuthToken { get; set; }

        public string Fr8AccountId { get; set; }

        public ActivityDTO[] ChildrenActions { get; set; }
        public int Ordering { get; set; }
        [JsonProperty("DocumentationSupport")]
        public string DocumentationSupport { get; set; }
    }
}
