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
        }

        /*
        public override void Authorize(string userId = null)
        {
            _userId = userId;
            _restfulServiceClient.AddRequestSignature(new HubAuthenticationPDHeaderSignature(TerminalToken, userId));
        }*/
    }
}