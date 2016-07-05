using Newtonsoft.Json;

namespace HubWeb.ViewModels.RequestParameters
{
    public class PlansPostParams
    {
        [JsonProperty("solution_name")]
        public string SolutionName { get; set; }
        [JsonProperty("update_registrations")]
        public bool UpdateRegistrations { get; set; } = false;
    }
}