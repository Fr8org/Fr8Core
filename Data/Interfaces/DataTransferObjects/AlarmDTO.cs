using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces.DataTransferObjects
{
    public class AlarmDTO
    {
        public string startTime { get; set; }
        public string containerId { get; set; }
        public string terminalName { get; set; }
        public string terminalVersion { get; set; }
    }
}
