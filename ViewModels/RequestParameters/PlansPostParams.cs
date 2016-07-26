using Newtonsoft.Json;

namespace HubWeb.ViewModels.RequestParameters
{
    public class PlansPostParams
    {
        /// <summary>
        /// Name of solution to create if specified
        /// </summary>
        public string solutionName { get; set; }
        /// <summary>
        /// Deprecated
        /// </summary>
        public bool updateRegistrations { get; set; } = false;
    }
}