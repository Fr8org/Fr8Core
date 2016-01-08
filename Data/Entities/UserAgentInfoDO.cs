using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class UserAgentInfoDO : BaseObject
    {
        [Key]
        public int Id { get; set; }
        
        public string RequestingURL { get; set; }

        public string DeviceFamily { get; set; }
        public bool DeviceIsSpider { get; set; }

        public String OSFamily { get; set; }
        public String OSMajor { get; set; }
        public String OSMinor { get; set; }
        public String OSPatch { get; set; }
        public String OSPatchMinor { get; set; }

        public String AgentFamily { get; set; }
        public String AgentMajor { get; set; }
        public String AgentMinor { get; set; }
        public String AgentPatch { get; set; }

        [ForeignKey("User")]
        public String UserID { get; set; }
        public Fr8AccountDO User { get; set; }
    }
}
