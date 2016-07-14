using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Fr8.Infrastructure.Interfaces;
using Fr8.TerminalBase.Services;

namespace PlanDirectory.Infrastructure
{
    public class PlanDirectoryHubCommunicator : DefaultHubCommunicator
    {
        public PlanDirectoryHubCommunicator(IRestfulServiceClient restfulServiceClient, string apiUrl, string token, string userId) : base(restfulServiceClient, apiUrl, token, userId)
        {
            //nasty hack for now -
            //we should already change this static authentication mechanism between hub and planDirectory
            _restfulServiceClient.ClearSignatures();
            _restfulServiceClient.AddRequestSignature(new HubAuthenticationPDHeaderSignature(TerminalToken, userId));
        }
    }
}