using Newtonsoft.Json;

namespace HubWeb.ViewModels.RequestParameters
{
    public class PlansPostParams
    {
        /// <summary>
        /// Name of solution to create if specified
        /// </summary>
        public string solution_name { get; set; }
        /// <summary>
        /// Deprecated
        /// </summary>
        public bool update_registrations { get; set; } = false;
    }
}