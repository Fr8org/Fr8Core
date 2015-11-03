using System;
using System.Collections.Generic;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hub.Managers.Crates
{
    partial class CrateStorageSerializer
    {
        public class CrateStorageSerializationProxy
        {
            [JsonProperty("crates")]
            public List<CrateSerializationProxy> Crates { get; set; }
        }

        
    }
}
