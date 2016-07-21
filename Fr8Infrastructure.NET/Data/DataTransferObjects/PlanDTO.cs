using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class PlanDTO
    {
        public PlanFullDTO Plan { get; set; }
    }

    public class PlanQueryDTO
    {
        /// <summary>
        /// Id of plan to retrieve
        /// </summary>
        [JsonProperty("id")]
        public Guid? Id { get; set; }
        /// <summary>
        /// Ordinal number of subset of plans to retrieve
        /// </summary>
        [JsonProperty("page")]
        public int? Page { get; set; }
        /// <summary>
        /// Name of property to order plans from. If preceeded with minus sign then sort will be performed descending order
        /// </summary>
        [JsonProperty("orderBy")]
        public string OrderBy { get; set; }
        /// <summary>
        /// Whether to perform sort of the results in descending order
        /// </summary>
        [JsonProperty("isDescending")]
        public bool? IsDescending { get; set; }
        /// <summary>
        /// Max number of plans to retrieve in a single response
        /// </summary>
        [JsonProperty("planPerPage")]
        public int? PlanPerPage { get; set; }
        /// <summary>
        /// Status to filter plans by
        /// </summary>
        [JsonProperty("status")]
        public int? Status { get; set; }
        /// <summary>
        /// Category to filter plans by
        /// </summary>
        [JsonProperty("category")]
        public string Category { get; set; }
        /// <summary>
        /// Part of name to filter by
        /// </summary>
        [JsonProperty("filter")]
        public string Filter { get; set; }
    }

    public class PlanResultDTO
    {
        [JsonProperty("plans")]
        public IList<PlanEmptyDTO> Plans { get; set; }

        [JsonProperty("currentPage")]
        public int CurrentPage { get; set; }

        [JsonProperty("totalPlanCount")]
        public int TotalPlanCount { get; set; }
    }
}
