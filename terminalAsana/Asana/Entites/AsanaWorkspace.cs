using Newtonsoft.Json;

namespace terminalAsana.Asana.Entites
{
    public class AsanaWorkspace
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("is_organization")]
        public bool IsOrganization { get; set; }
    }
}