using System.Threading.Tasks;
using Fr8.TerminalBase.Interfaces;

namespace Fr8.TerminalBase.Services
{
    /// <summary>
    /// Service that allows to gain access to multiple Hubs in those where the current terminal is registered.
    /// Service is registered as a singleton within the DI container.This service is available globally.
    /// See https://github.com/Fr8org/Fr8Core/blob/dev/Docs/ForDevelopers/SDK/.NET/Reference/IHubDiscoveryService.md
    /// </summary>
    public interface IHubDiscoveryService
    {
        Task<IHubCommunicator> GetHubCommunicator(string hubUrl);
        Task<IHubCommunicator> GetMasterHubCommunicator();
        void SetHubSecret(string hubUrl, string secret);
        Task<string[]> GetSubscribedHubs();
    }
}
