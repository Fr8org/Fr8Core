using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using System;
using System.Threading.Tasks;

namespace terminalAtlassian.Interfaces
{
    public interface IAtlassianSubscriptionManager
    {
        void CreateConnect(IHubCommunicator hubCommunicator, AuthorizationToken authToken);

        void CreateOrUpdatePolling(IHubCommunicator hubCommunicator, AuthorizationToken authToken);
    }
}