using System;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using DocuSign.eSign.Api;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.Services;
using terminalDocuSign.DataTransferObjects;

namespace terminalDocuSign.Controllers
{
    [RoutePrefix("authentication")]
    public class AuthenticationController : ApiController
    {
        private readonly IHubLoggerService _loggerService;

        public AuthenticationController(IHubLoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        [HttpPost]
        [Route("token")]
        public async Task<AuthorizationTokenDTO> GenerateInternalOAuthToken(CredentialsDTO curCredentials)
        {
            try
            {
                var authToken = await ObtainAuthToken(curCredentials);
                if (authToken == null)
                {
                    return new AuthorizationTokenDTO()
                    {
                        Error = "Unable to authenticate in DocuSign service, invalid login name or password."
                    };
                }

                var authorizationTokenDTO = new AuthorizationTokenDTO()
                {
                    Token = JsonConvert.SerializeObject(authToken),
                            ExternalAccountId = curCredentials.Username,
                            AuthCompletedNotificationRequired = true
                };

                string demoAccountStr = string.Empty;
                if (curCredentials.IsDemoAccount)
                {
                    demoAccountStr = CloudConfigurationManager.GetSetting("DemoAccountAttributeString");
                }

                if (authorizationTokenDTO.AdditionalAttributes == null)
                {
                    authorizationTokenDTO.AdditionalAttributes = demoAccountStr;
                }
                else if (!authorizationTokenDTO.AdditionalAttributes.Contains(demoAccountStr))
                {
                    authorizationTokenDTO.AdditionalAttributes += demoAccountStr;
                }
                return authorizationTokenDTO;
            }
            catch (Exception ex)
            {
                await _loggerService.ReportTerminalError(ex, curCredentials.Fr8UserId);

                return new AuthorizationTokenDTO()
                {
                    Error = "An error occurred while trying to authorize, please try again later."
                };
            }
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
    }
}