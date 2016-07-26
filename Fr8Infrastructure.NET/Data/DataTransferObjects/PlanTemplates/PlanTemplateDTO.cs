using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Fr8.Infrastructure.Data.DataTransferObjects.PlanTemplates
{
    public  class PlanTemplateDTO
    {

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("startingPlanNodeDescriptionId")]
        public Guid? StartingPlanNodeDescriptionId { get; set; }

        [JsonProperty("planNodeDescriptions")]
        public List<PlanNodeDescriptionDTO> PlanNodeDescriptions { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
