using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fr8Data.DataTransferObjects
{
    public class ValidationResultDTO
    {
        // list of controls that values are responsible for validation failure. Can be empty or null of corresponding error is activity-wide.
        [JsonProperty("controlNames")]
        public List<string> ControlNames { get; set; } = new List<string>();
        // validation error message
        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; } 
    }
}
