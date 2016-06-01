using System.Collections.Generic;

namespace Fr8Data.DataTransferObjects
{
    /// <summary>
    /// Plan DTO that contains full graph of other DTO objects
    /// Specifically used in Workflow Designer to draw entire process.
    /// </summary>
    public class PlanFullDTO : PlanEmptyDTO
    {
        /// <summary>
        /// List of SubPlan DTOs.
        /// </summary>
        public IEnumerable<FullSubplanDto> SubPlans { get; set; }

        public string Fr8UserId { get; set; }
    }

    /// <summary>
    /// SubPlan DTO that contains full graph of other DTO objects.
    /// </summary>
    public class FullSubplanDto : SubplanDTO
    {
        /// <summary>
        /// List of ActionList DTOs.
        /// </summary>
        public List<ActivityDTO> Activities { get; set; }
    }
}