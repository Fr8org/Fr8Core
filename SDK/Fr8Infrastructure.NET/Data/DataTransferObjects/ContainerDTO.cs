using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Fr8.Infrastructure.Data.Constants;
using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class ContainerDTO
    {
        [Required]
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("planId")]
        public Guid PlanId { get; set; }

        [JsonProperty("state")]
        public int State;

        [JsonProperty("lastUpdated")]
        public DateTimeOffset LastUpdated { get; set; }

        [JsonProperty("createDate")]
        public DateTimeOffset CreateDate { get; set; }

        [JsonProperty("currentActivityResponse")]
        public ActivityResponse? CurrentActivityResponse { get; set; }

        [JsonProperty("currentClientActivityName")]
        public string CurrentClientActivityName { get; set; }

        [JsonProperty("currentPlanType")]
        public PlanType? CurrentPlanType { get; set; }

        [JsonProperty("validationErrors")]
        public Dictionary<Guid, ValidationErrorsDTO> ValidationErrors { get; set; } = new Dictionary<Guid, ValidationErrorsDTO>();
    }
}
