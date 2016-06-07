using System.Collections.Generic;
using Newtonsoft.Json;

namespace fr8.Infrastructure.Data.DataTransferObjects
{
    public class ActivityTemplateCategoryDTO
    {
     
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("activities")]
        public IEnumerable<ActivityTemplateDTO> Activities { get; set; }

    }
}
