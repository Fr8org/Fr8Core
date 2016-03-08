using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class CrateDescriptionDTO
    {
        public CrateDescriptionDTO()
        {
            this.Selected = false;
        }

        [JsonProperty("manifestId")]
        public int ManifestId { get; set; }

        [JsonProperty("manifestType")]
        public string ManifestType{ get; set; }

        [JsonProperty("label")]
        public string Label{ get; set; }

        [JsonProperty("producedBy")]
        public string ProducedBy { get; set; }

        [JsonProperty("selected")]
        public bool Selected { get; set; }
    }
}
