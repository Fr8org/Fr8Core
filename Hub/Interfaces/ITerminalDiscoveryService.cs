using System.Threading.Tasks;

namespace Hub.Interfaces
{
    public interface ITerminalDiscoveryService
    {
        Task Discover();
        Task Discover(string terminalUrl);
    }
}