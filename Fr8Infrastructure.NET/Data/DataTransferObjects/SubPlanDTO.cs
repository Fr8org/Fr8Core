using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    /// <summary>
    /// Data transfer object for SubPlanDO entity.
    /// </summary>
    public class SubplanDTO
    {
        [JsonProperty("id")]
        public Guid? SubPlanId { get; set; }

        [JsonProperty("planId")]
        public Guid? PlanId { get; set; }

        [JsonProperty("parentPlanNodeId")]
        public Guid? ParentId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}