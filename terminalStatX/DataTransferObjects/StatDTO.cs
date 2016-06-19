using System.Collections.Generic;

namespace terminalStatX.DataTransferObjects
{
    public class StatDTO 
    {
        public StatDTO()
        {
            StatItems = new List<StatItemDTO>();
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public string Notes { get; set; }
        public string VisualType { get; set; }
        public string Value { get; set; }
        public string LastUpdatedDateTime { get; set; }
        public List<StatItemDTO> StatItems { get; set; } 
    }

    public class StatItemDTO
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}