using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Data.Interfaces.DataTransferObjects;
using StructureMap;
using TerminalBase.BaseClasses;
using terminalDocuSign.Interfaces;
using Utilities.Configuration.Azure;
using terminalDocuSign.DataTransferObjects;
using Hub.Managers.APIManagers.Transmitters.Restful;

namespace terminalDocuSign.Controllers
{
    [RoutePrefix("authentication")]
    public class AuthenticationController : BaseTerminalController
    {
        private const string curTerminal = "terminalDocuSign";


        [HttpPost]
        [Route("internal")]
        public async Task<AuthorizationTokenDTO> GenerateInternalOAuthToken(CredentialsDTO curCredentials)
        {
            try
            {
                // choose configuration either for demo or prod service 
                string endpoint = string.Empty;
                if (curCredentials.IsDemoAccount)
                {
                    endpoint = CloudConfigurationManager.GetSetting("endpoint");
                }
                else
                {
                    endpoint = CloudConfigurationManager.GetSetting("endpoint");
                }
                
                // Auth sequence according to https://www.docusign.com/p/RESTAPIGuide/RESTAPIGuide.htm#OAuth2/OAuth2%20Token%20Request.htm
                var oauthToken = await ObtainOAuthToken(curCredentials, endpoint);

                if (string.IsNullOrEmpty(oauthToken))
                {
                    return new AuthorizationTokenDTO()
                    {
                        Error = "Unable to authenticate in DocuSign service, invalid login name or password."
                    };
                }

                var docuSignAuthDTO = new DocuSignAuthTokenDTO()
                {
                    Email = curCredentials.Username,
                    ApiPassword = oauthToken
                };

                var authorizationTokenDTO = new AuthorizationTokenDTO()
                    {
                        Token = JsonConvert.SerializeObject(docuSignAuthDTO),
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
                ReportTerminalError(curTerminal, ex);

                return new AuthorizationTokenDTO()
                {
                    Error = "An error occurred while trying to authorize, please try again later."
                };
            }
        }

        private async Task<string> ObtainOAuthToken(CredentialsDTO curCredentials, string baseUrl)
        {
            var client = ObjectFactory.GetInstance<IRestfulServiceClient>();
            try
            {
                var response = await client
                .PostAsync(new Uri(new Uri(baseUrl), "oauth2/token"),
                    (HttpContent)new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("grant_type", "password"),
                        new KeyValuePair<string, string>("client_id", CloudConfigurationManager.GetSetting("DocuSignIntegratorKey")),
                        new KeyValuePair<string, string>("username", curCredentials.Username),
                        new KeyValuePair<string, string>("password", curCredentials.Password),
                        new KeyValuePair<string, string>("scope", "api"),
                    }));

                var responseObject = JsonConvert.DeserializeAnonymousType(response, new { access_token = "" });

                return responseObject.access_token;
            }
            catch (Exception ex)
            {
                ReportTerminalError("terminalDocuSign", ex);
                return null;
            }
        }
    }
}