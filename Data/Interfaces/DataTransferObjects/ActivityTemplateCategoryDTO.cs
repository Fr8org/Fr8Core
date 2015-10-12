using System.Collections.Generic;
using Data.States;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class ActivityTemplateCategoryDTO
    {
     
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("activities")]
        public IEnumerable<ActivityTemplateDTO> Activities { get; set; }

    }
}
