using System.Threading.Tasks;
using Fr8.TerminalBase.Interfaces;

namespace Fr8.TerminalBase.Services
{
    public interface IHubDiscoveryService
    {
        Task<IHubCommunicator> GetHubCommunicator(string hubUrl);
        Task<IHubCommunicator> GetMasterHubCommunicator();
        void SetHubSecret(string hubUrl, string secret);
        Task RequestDiscovery();
    }
}
