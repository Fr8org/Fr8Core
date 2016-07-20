using System;
using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class ActivityCategoryDTO
    {
        public ActivityCategoryDTO() { }

        public ActivityCategoryDTO(string name, string iconPath)
        {
            Name = name;
            IconPath = iconPath;
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("iconPath")]
        public string IconPath { get; set; }
    }
}
