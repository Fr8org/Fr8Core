using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;

namespace Fr8.TerminalBase.Services
{
    /// <summary>
    /// Service that allows to broadcast event crates to multiple Hubs where current terminal is registered.
    /// Service is registered as a singleton within the DI container.This service is available globally.
    /// See https://github.com/Fr8org/Fr8Core/blob/dev/Docs/ForDevelopers/SDK/.NET/Reference/IHubEventReporter.md
    /// </summary>
    public interface IHubEventReporter
    {
        Task Broadcast(Crate eventPayload);
    }
}