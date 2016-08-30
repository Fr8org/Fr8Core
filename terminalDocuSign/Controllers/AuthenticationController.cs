using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Newtonsoft.Json;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using DocuSign.eSign.Api;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.Services;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services.New_Api;

namespace terminalDocuSign.Controllers
{
    [RoutePrefix("authentication")]
    public class AuthenticationController : ApiController
    {
        private readonly IHubLoggerService _loggerService;
        private readonly IDocuSignManager _docuSignManager;

        public AuthenticationController(IHubLoggerService loggerService, IDocuSignManager docuSignManager)
        {
            _loggerService = loggerService;
            _docuSignManager = docuSignManager;
        }

        [HttpPost]
        [Route("request_url")]
        public ExternalAuthUrlDTO GenerateOAuthInitiationURL()
        {
            var state = Guid.NewGuid();
            return new ExternalAuthUrlDTO
            {
                ExternalStateToken = state.ToString(),
                Url = $"{CloudConfigurationManager.GetSetting("terminalDocuSign.TerminalEndpoint")}/environmentSelection?state={state}"
            };
        }

        [HttpPost]
        [Route("token")]
        public async Task<AuthorizationTokenDTO> GenerateOAuthToken(ExternalAuthenticationDTO externalAuthDTO)
        {
            try
            {
                var codeRequestResult = ParseResponseParameters(externalAuthDTO.RequestQueryString);
                if (string.IsNullOrEmpty(codeRequestResult.Code) || string.IsNullOrEmpty(codeRequestResult.State))
                {
                    throw new ApplicationException("Code or State is empty");
                }
                var now = DateTime.UtcNow;
                var token = await _docuSignManager.RequestAccessToken(codeRequestResult.Code, codeRequestResult.State, codeRequestResult.IsDemo);
                token.ExpirationDate = now.AddSeconds(token.ExpiresIn);
                //TODO: request user info
                return new AuthorizationTokenDTO
                {
                    Token = JsonConvert.SerializeObject(token),
                    ExternalAccountId = "docusign_developer@dockyard.company",
                    ExternalAccountName = "docusign_developer@dockyard.company",
                    ExternalDomainId = string.Empty,
                    ExternalDomainName = string.Empty,
                    ExternalStateToken = codeRequestResult.State
                };
            }
            catch (Exception ex)
            {
                await _loggerService.ReportTerminalError(ex, externalAuthDTO.Fr8UserId);
                return new AuthorizationTokenDTO
                {
                    Error = "An error occurred while trying to authorize, please try again later."
                };
            }
        }

        private RequestCodeResult ParseResponseParameters(string queryString)
        {
            if (string.IsNullOrEmpty(queryString))
            {
                throw new ApplicationException("QueryString is empty.");
            }
            var code = string.Empty;
            var state = string.Empty;

            var tokens = queryString.Split(new[] {'&'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Split(new[] {'='}, StringSplitOptions.RemoveEmptyEntries))
                .ToDictionary(x => x[0], x => x[1]);
            var result = new RequestCodeResult();
            string value;
            if (tokens.TryGetValue("code", out value))
            {
                result.Code = value;
            }
            if (tokens.TryGetValue("state", out value))
            {
                result.State = value;
            }
            if (tokens.TryGetValue("demo", out value) && value == "1")
            {
                result.IsDemo = true;
            }
            return result;
        }

        private async Task<DocuSignAuthTokenDTO> ObtainAuthToken(CredentialsDTO curCredentials)
        {
            //TODO: choose configuration either for demo or prod service 
            string endpoint = string.Empty;
            if (curCredentials.IsDemoAccount)
            {
                endpoint = CloudConfigurationManager.GetSetting("environment_DEMO") + "/restapi";
            }
            else
            {
                endpoint = CloudConfigurationManager.GetSetting("environment") + "/restapi";
            }

            // Here we make a call to API with X-DocuSign-Authentication to retrieve both oAuth token and AccountID
            string integratorKey = CloudConfigurationManager.GetSetting("DocuSignIntegratorKey" + (curCredentials.IsDemoAccount ? "_DEMO" : ""));
            ApiClient apiClient = new ApiClient(endpoint);
            string authHeader = "{\"Username\":\"" + curCredentials.Username + "\", \"Password\":\"" + curCredentials.Password + "\", \"IntegratorKey\":\"" + integratorKey + "\"}";
            Configuration conf = new Configuration(apiClient);
            conf.AddDefaultHeader("X-DocuSign-Authentication", authHeader);
            AuthenticationApi authApi = new AuthenticationApi(conf);
            LoginInformation loginInfo = await authApi.LoginAsync(new AuthenticationApi.LoginOptions() { apiPassword = "true" });

            string accountId = loginInfo.LoginAccounts[0].AccountId; //it seems that althought one DocuSign account can have multiple users - only one is returned, the one that credentials were provided for
            DocuSignAuthTokenDTO result = new DocuSignAuthTokenDTO()
            {
                AccountId = accountId,
                ApiPassword = loginInfo.ApiPassword,
                Email = curCredentials.Username,
                IsDemoAccount = curCredentials.IsDemoAccount,
                Endpoint = loginInfo.LoginAccounts[0].BaseUrl.Replace("v2/accounts/" + accountId.ToString(), "")
            };

            return result;
        }

        private class RequestCodeResult
        {
            public string Code { get; set; }

            public string State { get; set; }

            public bool IsDemo { get; set; }
        }
    }
}