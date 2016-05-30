using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8Data.Crates;

namespace TerminalBase.Models
{
    public class ContainerExecutionContext
    {
        public ICrateStorage PayloadStorage { get; set; }
        public Guid ContainerId { get; set; }
    }
}
