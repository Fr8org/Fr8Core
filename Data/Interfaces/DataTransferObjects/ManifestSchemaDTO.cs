using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces.DataTransferObjects
{
    public class EventReportMS
    {
        public string EventNames { get; set; }
        public string ProcessDOId { get; set; }
        public List<CrateDTO> EventPayload { get; set; }
    }
}
