using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using HealthMonitor.Utility;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace terminaBaselTests.Tools.Terminals
{
    public class IntegrationTestTools_terminalDocuSign
    {
        private readonly BaseHubIntegrationTest _baseHubITest;

        public IntegrationTestTools_terminalDocuSign(BaseHubIntegrationTest baseHubIntegrationTest)
        {
            _baseHubITest = baseHubIntegrationTest;
        }

        public async Task<AuthorizationTokenDTO> GenerateAuthToken(string username, string password, int terminalId)
        {
            var creds = new CredentialsDTO()
            {
                Username = username,
                Password = password, 
                IsDemoAccount = true,
                TerminalId = terminalId
            };

            var token = await _baseHubITest.HttpPostAsync<CredentialsDTO, JObject>(_baseHubITest.GetHubApiBaseUrl() + "authentication/token", creds);
        
            Assert.AreNotEqual(token["error"].ToString(), "Unable to authenticate in DocuSign service, invalid login name or password.",
                "DocuSign auth error. Perhaps max number of tokens is exceeded.");
            Assert.AreEqual(false, String.IsNullOrEmpty(token["authTokenId"].Value<string>()),
                "AuthTokenId is missing in API response.");

            Guid tokenGuid = Guid.Parse(token["authTokenId"].Value<string>());

            return await Task.FromResult(new AuthorizationTokenDTO { Token = tokenGuid.ToString() });
        }
    }
}
