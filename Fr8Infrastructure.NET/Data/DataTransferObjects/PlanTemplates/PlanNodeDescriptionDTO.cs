using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Fr8.Infrastructure.Data.DataTransferObjects.PlanTemplates
{
    public class PlanNodeDescriptionDTO
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("parentNodeId")]
        public Guid? ParentNodeId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("transitions")]
        public List<NodeTransitionDTO> Transitions { get; set; }

        [JsonProperty("activityDescription")]
        public ActivityDescriptionDTO ActivityDescription { get; set; }

        [JsonProperty("subPlanName")]
        public string SubPlanName { get; set; }

        [JsonProperty("subPlanOriginalId")]
        public string SubPlanOriginalId { get; set; }

        [JsonProperty("isStartingSubplan")]
        public bool IsStartingSubplan { get; set; }
    }
}
