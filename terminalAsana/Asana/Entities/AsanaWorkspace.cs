using Newtonsoft.Json;
using terminalAsana.Interfaces;

namespace terminalAsana.Asana.Entities
{
    public class AsanaWorkspace : IAsanaEntity
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("is_organization")]
        public bool IsOrganization { get; set; }
    }
}