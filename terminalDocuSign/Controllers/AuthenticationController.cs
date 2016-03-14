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
using Hub.Managers.APIManagers.Transmitters.Restful;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services;

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
            var docuSignAuth = new DocuSignAuthentication();
            try
            {
                var authToken = await docuSignAuth.ObtainOAuthToken(
                    curCredentials.Username,
                    curCredentials.Password,
                    baseUrl
                );
                return authToken;
            }
            catch (Exception ex)
            {
                ReportTerminalError("terminalDocuSign", ex);
                return null;
            }
        }
    }
}