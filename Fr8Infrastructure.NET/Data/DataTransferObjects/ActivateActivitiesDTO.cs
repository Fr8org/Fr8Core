using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class ActivateActivitiesDTO
    {
        [JsonProperty("validationErrors")]
        public Dictionary<Guid, ValidationErrorsDTO> ValidationErrors { get; set; } = new Dictionary<Guid, ValidationErrorsDTO>();
    }
}
