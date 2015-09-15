using System.Collections.Generic;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class PluginDiscoveryDTO
    {
        
        public bool Success { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public List<ActivityTemplateDTO> Activities { get; set; }




    }
}
