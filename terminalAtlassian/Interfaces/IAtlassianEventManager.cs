using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Models;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using StructureMap;
using System;

namespace terminalAtlassian.Interfaces
{
    public interface IAtlassianEventManager : IDisposable
    {
        Task<Crate> ProcessExternalEvents(string curExternalEventPayload);
        Task<Crate> ProcessInternalEvents(IContainer container, string curExternalEventPayload);
    }
}