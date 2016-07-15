using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;

namespace Fr8.TerminalBase.Services
{
    public interface IHubEventReporter
    {
        Task Broadcast(Crate eventPayload);
    }
}