using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using HealthMonitor.Utility;
using NUnit.Framework;

namespace terminaBaselTests.Tools.Terminals
{
    public class IntegrationTestTools_terminalGoogle
    {

        private readonly BaseHubIntegrationTest _baseHubITest;

        public IntegrationTestTools_terminalGoogle(BaseHubIntegrationTest baseHubIntegrationTest)
        {
            _baseHubITest = baseHubIntegrationTest;
        }

        public async Task<Guid> ExtractGoogleDefaultToken()
        {
            var tokens = await _baseHubITest.HttpGetAsync<IEnumerable<ManageAuthToken_Terminal>>(
                _baseHubITest.GetHubApiBaseUrl() + "manageauthtoken/"
            );

            Assert.NotNull(tokens, "No authorization tokens were found for the integration testing user.");

            var terminal = tokens.FirstOrDefault(x => x.Name == "terminalGoogle");
            Assert.NotNull(terminal, "No authorization tokens were found for the terminalGoogle.");

            var token = terminal.AuthTokens.FirstOrDefault(x => x.IsMain);
            Assert.NotNull(token, "Authorization token for Google is not found for the integration testing user.Please go to the target instance of fr8 and log in with the integration testing user credentials.Then add a Google action to any plan and be sure to set the 'Use for all Activities' checkbox on the Authorize Accounts dialog while authenticating") ;

            return token.Id;
        }
     }
}
