using System;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using StructureMap;

namespace Fr8.Testing.Integration.Tools.Terminals
{
    public class IntegrationTestTools_terminalDocuSign
    {
        private readonly BaseHubIntegrationTest _baseHubITest;

        public IntegrationTestTools_terminalDocuSign(BaseHubIntegrationTest baseHubIntegrationTest)
        {
            _baseHubITest = baseHubIntegrationTest;
        }

        /// <summary>
        /// Generate new DocuSign authorization token from provided credentials
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="terminalId"></param>
        /// <returns></returns>
        public async Task<AuthorizationTokenDTO> GenerateAuthToken(string username, string password, TerminalSummaryDTO terminalDTO)
        {
            var creds = new CredentialsDTO()
            {
                Username = username,
                Password = password, 
                IsDemoAccount = true,
                Terminal = terminalDTO
            };

            var token = await _baseHubITest.HttpPostAsync<CredentialsDTO, JObject>(_baseHubITest.GetHubApiBaseUrl() + "authentication/token", creds);
        
            Assert.AreNotEqual(token["error"].ToString(), "Unable to authenticate in DocuSign service, invalid login name or password.",
                "DocuSign auth error. Perhaps max number of tokens is exceeded.");
            Assert.AreEqual(false, String.IsNullOrEmpty(token["authTokenId"].Value<string>()),
                "AuthTokenId is missing in API response.");

            Guid tokenGuid = Guid.Parse(token["authTokenId"].Value<string>());

            return await Task.FromResult(new AuthorizationTokenDTO { Token = tokenGuid.ToString() });
        }

        /// <summary>
        /// Authenticate to DocuSign and accociate generated token with activity
        /// </summary>
        /// <param name="activityId"></param>
        /// <param name="credentials"></param>
        /// <param name="terminalId"></param>
        /// <returns></returns>
        public async Task<Guid> AuthenticateDocuSignAndAssociateTokenWithAction(Guid activityId, CredentialsDTO credentials, TerminalSummaryDTO terminalDTO)
        {
            //
            // Authenticate with DocuSign Credentials
            //
            var authTokenDTO = await GenerateAuthToken(credentials.Username, credentials.Password, terminalDTO);
            var tokenGuid = Guid.Parse(authTokenDTO.Token);
           
            //
            // Asociate token with action
            //
            var applyToken = new AuthenticationTokenGrantDTO()
            {
                ActivityId = activityId,
                AuthTokenId = tokenGuid,
                IsMain = true
            };
            await _baseHubITest.HttpPostAsync<AuthenticationTokenGrantDTO[], string>(_baseHubITest.GetHubApiBaseUrl() + "authentication/tokens/grant", new[] { applyToken });

            return tokenGuid;
        }

        
        /// <summary>
        /// Get GoogleAuthToken from the TokenRepository based on authorizationTokenId 
        /// </summary>
        /// <param name="authorizationTokenId"></param>
        /// <returns></returns>
        public AuthorizationTokenDO GetDocuSignAuthToken(Guid authorizationTokenId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var validToken = uow.AuthorizationTokenRepository.FindTokenById(authorizationTokenId);

                Assert.IsNotNull(validToken, "Reading default docusSign token from AuthorizationTokenRepository failed. Please provide default account for authenticating terminalDocuSign.");

                return validToken;
            }
        }
    }
}
