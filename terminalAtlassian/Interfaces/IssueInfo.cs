using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;

namespace terminalAtlassian.Interfaces
{
    public class IssueInfo
    {
        public string Key { get; set; }

        public string ProjectKey { get; set; }

        public string IssueTypeKey { get; set; }

        public string PriorityKey { get; set; }

        public string Summary { get; set; }

        public string Description { get; set; }

        public List<FieldDTO> CustomFields { get; set; }
    }
}