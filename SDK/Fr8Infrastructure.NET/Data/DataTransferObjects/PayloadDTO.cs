using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{   
    public class PayloadDTO
    {
        public PayloadDTO(Guid containerId)
        {
            ContainerId = containerId;
        }

        [JsonProperty("container")]
        public CrateStorageDTO CrateStorage { get; set; }

        [JsonProperty("containerId")]
        public Guid ContainerId { get; set; }
    }
}
