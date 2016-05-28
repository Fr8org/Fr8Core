using System;
using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;

namespace Fr8Data.DataTransferObjects
{
    public class ActivateActivitiesDTO
    {
        [JsonProperty("validationErrors")]
        public Dictionary<Guid, ValidationErrorsDTO> ValidationErrors { get; set; } = new Dictionary<Guid, ValidationErrorsDTO>();
    }
}
