using System.Collections.Generic;
using Newtonsoft.Json;

namespace fr8.Infrastructure.Data.DataTransferObjects
{
    public class TerminalDiscoveryDTO
    {
        
        public bool Success { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public List<ActivityTemplateDTO> Activities { get; set; }




    }
}
