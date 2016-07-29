using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Fr8.Infrastructure.Data.States;
using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class PlanEmptyDTO
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("lastUpdated")]
        public DateTimeOffset LastUpdated { get; set; }

        [JsonProperty("planState")]
        public int PlanState { get; set; }

        [JsonProperty("startingSubPlanId")]
        public Guid StartingSubPlanId { get; set; }

        [JsonProperty("visibility")]
        public PlanVisibilityDTO Visibility { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }
    }
}