using System.Collections.Generic;
using Fr8.Infrastructure.Data.Constants;

namespace Fr8.Infrastructure.Data.Manifests
{
    public class StatXItemCM : Manifest 
    {
        public StatXItemCM() : base(MT.StatXStatItem)
        {
            StatValueItems = new List<StatValueItemDTO>();
        }

        public string Id { get; set; }
        public string VisualType { get; set; }
        public string Value { get; set; }
        public string LastUpdatedDateTime { get; set; }
        public List<StatValueItemDTO> StatValueItems { get; set; }
    }

    public class StatValueItemDTO
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
