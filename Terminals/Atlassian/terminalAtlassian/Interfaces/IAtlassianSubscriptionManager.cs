using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;

namespace terminalAtlassian.Interfaces
{
    public interface IAtlassianSubscriptionManager
    {
        void CreateConnect(IHubCommunicator hubCommunicator, AuthorizationToken authToken);
    }
}