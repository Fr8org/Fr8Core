using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace fr8.Infrastructure.Data.DataTransferObjects
{   
    public class PayloadDTO
    {
        public PayloadDTO(Guid containerId)
        {
            ContainerId = containerId;
        }

        [JsonProperty("container")]
        public CrateStorageDTO CrateStorage { get; set; }

        public Guid ContainerId { get; set; }
    }
}
