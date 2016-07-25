using Newtonsoft.Json;
using System;

namespace Fr8.Infrastructure.Data.DataTransferObjects.PlanTemplates
{
    public class ActivityDescriptionDTO
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("originalId")]
        public string OriginalId { get; set; }

        [JsonProperty("crateStorage")]
        public string CrateStorage { get; set; }

        [JsonProperty("activityTemplateId")]
        public Guid ActivityTemplateId { get; set; }
    }
}
