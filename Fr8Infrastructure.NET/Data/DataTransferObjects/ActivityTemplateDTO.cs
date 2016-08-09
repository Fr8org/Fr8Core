using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Fr8.Infrastructure.Data.States;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class ActivityTemplateDTO
    {
        private sealed class IdEqualityComparer : IEqualityComparer<ActivityTemplateDTO>
        {
            public bool Equals(ActivityTemplateDTO x, ActivityTemplateDTO y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Id.Equals(y.Id);
            }

            public int GetHashCode(ActivityTemplateDTO obj)
            {
                return obj.Id.GetHashCode();
            }
        }

        private static readonly IEqualityComparer<ActivityTemplateDTO> IdComparerInstance = new IdEqualityComparer();

        public static IEqualityComparer<ActivityTemplateDTO> IdComparer => IdComparerInstance;

        public ActivityTemplateDTO()
        {
            Type = ActivityType.Standard;
        }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("terminal")]
        public TerminalDTO Terminal { get; set; }

        [JsonProperty("tags")]
        public string Tags { get; set; }

        [JsonProperty("categories")]
        public ActivityCategoryDTO[] Categories { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ActivityType Type { get; set; }

        [JsonProperty("minPaneWidth")]
        public int MinPaneWidth { get; set; }

        [JsonProperty("needsAuthentication")]
        public bool NeedsAuthentication { get; set; }

        [JsonProperty("showDocumentation")]
        public ActivityResponseDTO ShowDocumentation { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
