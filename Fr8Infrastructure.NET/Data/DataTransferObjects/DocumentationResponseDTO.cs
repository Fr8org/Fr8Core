using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class DocumentationResponseDTO
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public double Version { get; set; }

        [JsonProperty("terminal")]
        //TODO: To be changed with another type
        public string Terminal { get; set; }

        [JsonProperty("body")]
        //This field is to hold an HTML
        public string Body { get; set; }

        public DocumentationResponseDTO()
        {
        }

        public DocumentationResponseDTO(string body)
        {
            Body = body;
        }

        public DocumentationResponseDTO(string name, double version, string terminal, string body)
        {
            Name = name;
            Version = version;
            Terminal = terminal;
            Body = body;
        }
    }

}
