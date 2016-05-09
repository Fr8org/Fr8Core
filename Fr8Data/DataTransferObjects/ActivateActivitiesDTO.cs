using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fr8Data.DataTransferObjects
{
    public class ActivateActivitiesDTO
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }

        public ContainerDTO Container { get; set; }

        [JsonProperty("redirectToPlan")]
        public bool RedirectToPlanBuilder { get; set; }
    
        [JsonProperty("activitiesCollection")]
        public List<ActivityDTO> ActivitiesCollections { get; set; } 
    }
}
