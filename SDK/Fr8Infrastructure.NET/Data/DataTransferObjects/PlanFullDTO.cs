using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    /// <summary>
    /// Plan DTO that contains full graph of other DTO objects
    /// Specifically used in Workflow Designer to draw entire process.
    /// </summary>
    //public class PlanFullDTO : PlanNoChildrenDTO
    //{
    //    [JsonProperty("subPlans")]
    //    /// <summary>
    //    /// List of SubPlan DTOs.
    //    /// </summary>
    //    public IEnumerable<FullSubplanDto> SubPlans { get; set; }

    //    [JsonProperty("fr8UserId")]
    //    public string Fr8UserId { get; set; }
    //}

    /// <summary>
    /// SubPlan DTO that contains full graph of other DTO objects.
    /// </summary>
    public class FullSubplanDto : SubplanDTO
    {
        [JsonProperty("activities")]
        /// <summary>
        /// List of ActionList DTOs.
        /// </summary>
        public List<ActivityDTO> Activities { get; set; }
    }
}