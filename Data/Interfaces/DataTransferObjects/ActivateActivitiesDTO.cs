using System.Collections.Generic;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class ActivateActivitiesDTO
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }

        public ContainerDTO Container { get; set; }

        [JsonProperty("redirectToRoute")]
        public bool RedirectToRouteBuilder { get; set; }
    
        [JsonProperty("activitiesCollection")]
        public List<ActivityDTO> ActivitiesCollections { get; set; } 
    }
}
