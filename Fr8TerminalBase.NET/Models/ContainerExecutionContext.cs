using System;
using fr8.Infrastructure.Data.Crates;

namespace Fr8.TerminalBase.Models
{
    public class ContainerExecutionContext
    {
        public ICrateStorage PayloadStorage { get; set; }
        public Guid ContainerId { get; set; }
    }
}
