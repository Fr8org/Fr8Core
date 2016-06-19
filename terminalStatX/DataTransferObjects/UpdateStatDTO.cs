using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace terminalStatX.DataTransferObjects
{
    public class UpdateStatDTO
    {
        [JsonProperty("lastUpdatedDateTime")]
        public DateTime LastUpdatedDateTime { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class UpdateStatWithItemsDTO
    {
        public UpdateStatWithItemsDTO()
        {
            Items = new List<StatItemValueDTO>();
        }

        [JsonProperty("lastUpdatedDateTime")]
        public DateTime LastUpdatedDateTime { get; set; }

        [JsonProperty("items")]
        public List<StatItemValueDTO> Items { get; set; }
    }

    public class StatItemValueDTO
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}