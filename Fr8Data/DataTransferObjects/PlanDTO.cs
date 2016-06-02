using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fr8Data.DataTransferObjects
{
    public class PlanDTO
    {
        public PlanFullDTO Plan { get; set; }
    }

    public class PlanQueryDTO
    {
        [JsonProperty("id")]
        public Guid? Id { get; set; }

        [JsonProperty("page")]
        public int? Page { get; set; }

        [JsonProperty("orderBy")]
        public string OrderBy { get; set; }

        [JsonProperty("isDescending")]
        public bool? IsDescending { get; set; }

        [JsonProperty("planPerPage")]
        public int? PlanPerPage { get; set; }

        [JsonProperty("status")]
        public int? Status { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("filter")]
        public string Filter { get; set; }
    }

    public class PlanResultDTO
    {
        [JsonProperty("plans")]
        public IList<PlanNoChildrenDTO> Plans { get; set; }

        [JsonProperty("currentPage")]
        public int CurrentPage { get; set; }

        [JsonProperty("totalPlanCount")]
        public int TotalPlanCount { get; set; }
    }
}
