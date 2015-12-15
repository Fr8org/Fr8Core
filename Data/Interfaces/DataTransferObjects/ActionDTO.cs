using System;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class ActionDTO : ActionDTOBase
    {
        public Guid? ParentRouteNodeId { get; set; }

        public CrateStorageDTO CrateStorage { get; set; }
        
        public int? ActivityTemplateId { get; set; }
        
        public string Label { get; set; }

        [JsonProperty("activityTemplate")]
        public ActivityTemplateDTO ActivityTemplate { get; set; }

        [JsonProperty("isTempId")]
        public bool IsTempId { get; set; }

        public bool IsExplicitData { get; set; }

        public string ExplicitData { get; set; }

        [JsonProperty("action_version")]
        public string ActionVersion { get; set; }

        public string CurrentView { get; set; }

        public Guid ContainerId { get; set; }

        public AuthorizationTokenDTO AuthToken { get; set; }

        public ActionDTO[] ChildrenActions { get; set; }
    }
}
