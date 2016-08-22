using System;
using System.Threading.Tasks;
using Fr8.TerminalBase.Models;
using StructureMap;
using System.Collections.Generic;
using Fr8.Infrastructure.Data.Crates;

namespace terminalInstagram.Interfaces
{
    public interface IInstagramEventManager : IDisposable
    {
        Task Subscribe();
        Task<Crate> ProcessUserEvents(IContainer container, string curExternalEventPayload);
    }
}
