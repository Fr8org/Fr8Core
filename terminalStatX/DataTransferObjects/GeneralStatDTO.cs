using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using terminalStatX.Helpers;
using terminalStatX.Infrastructure;

namespace terminalStatX.DataTransferObjects
{
    public class GeneralStatDTO : BaseStatDTO
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class GeneralStatWithItemsDTO : BaseStatDTO
    {
        public GeneralStatWithItemsDTO()
        {
            Items = new List<StatItemValueDTO>();
        }

        [JsonProperty("items")]
        public List<StatItemValueDTO> Items { get; set; }
    }

    public class StatItemValueDTO
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("checked")]
        public bool Checked {get;set;}
    }
}