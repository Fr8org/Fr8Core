using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Fr8Data.DataTransferObjects
{
    /// <summary>
    /// This entity represents a DocuSign event that user may subscribe to
    /// </summary>
    public class ExternalEventDTO
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        public ExternalEventDTO(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
