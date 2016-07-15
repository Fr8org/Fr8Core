using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Testing.Integration;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;
using terminalStatX.DataTransferObjects;

namespace terminalStatXTests.TestTools
{
    public class AuthorizationTokenHelpers
    {
        private readonly BaseHubIntegrationTest _baseHubTest;

        public AuthorizationTokenHelpers(BaseHubIntegrationTest baseHubTest)
        {
            _baseHubTest = baseHubTest;
        }

        public async Task<AuthorizationTokenDTO> GetStatXAuthToken()
        {
            var tokens = await _baseHubTest.HttpGetAsync<IEnumerable<AuthenticationTokenTerminalDTO>>(
                _baseHubTest.GetHubApiBaseUrl() + "authentication/tokens"
            );

            Assert.NotNull(tokens, "No authorization tokens were found for the integration testing user.");

            var terminal = tokens.FirstOrDefault(x => x.Name == "terminalStatX");
            Assert.NotNull(terminal, "No authorization tokens were found for the terminalStatX.");

            var token = terminal.AuthTokens.FirstOrDefault(x => x.IsMain);
            Assert.NotNull(token, "Authorization token for StatX is not found for the integration testing user.Please go to the target instance of fr8 and log in with the integration testing user credentials.Then add a StatX action to any plan and be sure to set the 'Use for all Activities' checkbox on the Authorize Accounts dialog while authenticating");

            var authorizationTokenId = token.Id;

            Debug.WriteLine($"Getting statX auth token for authorizationTokenId: {authorizationTokenId}");
            Assert.IsNotNull(authorizationTokenId, "The statX authorization token is null");
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var validToken = uow.AuthorizationTokenRepository.FindTokenById(authorizationTokenId);

                Assert.IsNotNull(validToken, "Reading default statX token from AuthorizationTokenRepository failed. Please provide default account for authenticating terminalstatX.");

                return new AuthorizationTokenDTO()
                {
                    Token = validToken.Token
                };
            }
        }
    }
}
