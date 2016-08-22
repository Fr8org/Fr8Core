using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.Interfaces.DataTransferObjects;
using Fr8Data.Constants;
using Newtonsoft.Json;

namespace Fr8Data.DataTransferObjects
{
    public class ContainerDTO
    {
        [Required]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid PlanId { get; set; }
        public int State;

        public DateTimeOffset LastUpdated { get; set; }
        public DateTimeOffset CreateDate { get; set; }

        public ActivityResponse? CurrentActivityResponse { get; set; }
        public string CurrentClientActivityName { get; set; }
        public PlanType? CurrentPlanType { get; set; }

        [JsonProperty("validationErrors")]
        public Dictionary<Guid, ValidationErrorsDTO> ValidationErrors { get; set; } = new Dictionary<Guid, ValidationErrorsDTO>();
    }
}
