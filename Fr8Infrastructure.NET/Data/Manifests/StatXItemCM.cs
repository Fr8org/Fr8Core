using System.Collections.Generic;
using Fr8.Infrastructure.Data.Constants;
using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.Manifests
{
    public class StatXItemCM : Manifest 
    {
        public StatXItemCM() : base(MT.StatXItem)
        {
            StatValueItems = new List<StatValueItemDTO>();
        }

        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("visualType")]
        public string VisualType { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }
        [JsonProperty("currentIndex")]
        public int CurrentIndex { get; set; }
        [JsonProperty("lastUpdatedDateTime ")]
        public string LastUpdatedDateTime { get; set; }
        [JsonProperty("items")]
        public List<StatValueItemDTO> StatValueItems { get; set; }
    }

    public class StatValueItemDTO
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }
        [JsonProperty("checked")]
        public bool Checked { get; set; }
    }
}
