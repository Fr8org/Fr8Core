using System.Threading.Tasks;

namespace Hub.Interfaces
{
    public interface ITerminalDiscoveryService
    {
        Task Discover();
        Task<bool> Discover(string terminalUrl);
        Task RegisterTerminal(string endpoint);
    }
}