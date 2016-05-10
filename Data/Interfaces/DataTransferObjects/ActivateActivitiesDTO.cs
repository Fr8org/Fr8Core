using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class ActivateActivitiesDTO
    {
        [JsonProperty("validationErrors")]
        public Dictionary<Guid, ValidationErrorsDTO> ValidationErrors { get; set; } = new Dictionary<Guid, ValidationErrorsDTO>();
    }
}
