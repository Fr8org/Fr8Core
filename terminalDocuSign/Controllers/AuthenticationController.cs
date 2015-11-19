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
                // Auth sequence according to https://www.docusign.com/p/RESTAPIGuide/RESTAPIGuide.htm#OAuth2/OAuth2%20Token%20Request.htm
                var oauthToken = await ObtainOAuthToken(curCredentials, CloudConfigurationManager.GetSetting("endpoint"));

                if (string.IsNullOrEmpty(oauthToken))
                {
                    return new AuthorizationTokenDTO()
                    {
                        Error = "Unable to authenticate in DocuSign service, invalid login name or password."
                    };
                }

                var docuSignAuthDTO = new DocuSignAuthDTO()
                {
                    Email = curCredentials.Username,
                    ApiPassword = oauthToken
                };

                return new AuthorizationTokenDTO()
                {
                    Token = JsonConvert.SerializeObject(docuSignAuthDTO),
                    ExternalAccountId = curCredentials.Username,
                    RequiresCallback = true
                };
            }
            catch (Exception ex)
            {
                ReportTerminalError(curTerminal, ex);

                return new AuthorizationTokenDTO()
                {
                    Error = "An error occured while trying to authenticate, please try again later."
                };
            }
        }

        [HttpPost]
        [Route("completed")]
        public async Task OnAuthenticationCompleted(AuthorizationTokenDTO curAuthorizationTokenDTO)
        {
            //upon successful DocuSign authentication, we should have fr8_user_id
            if (string.IsNullOrEmpty(curAuthorizationTokenDTO.AdditionalAttributes) ||
                !curAuthorizationTokenDTO.AdditionalAttributes.Contains("fr8_user_id"))
            {
                throw new ArgumentException(
                    "fr8_user_id argument is missing on completion of sucessfull DocuSign authentication");
            }

            string curFr8UserId =
                curAuthorizationTokenDTO.AdditionalAttributes.Split(new string[] {";"}, StringSplitOptions.None)
                    .ToList()
                    .Single(attribute => attribute.StartsWith("fr8_user_id"))
                    .Split(new string[] {"="}, StringSplitOptions.None)[1];

            //create Monitor All DocuSign Events route
            await ObjectFactory.GetInstance<IDocuSignRoute>().CreateRoute_MonitorAllDocuSignEvents(curFr8UserId);
        }

        private async Task<string> ObtainOAuthToken(CredentialsDTO curCredentials, string baseUrl)
        {
            var response = await CreateHttpClient(baseUrl)
                .PostAsync("oauth2/token",
                    new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("grant_type", "password"), 
                        new KeyValuePair<string, string>("client_id", CloudConfigurationManager.GetSetting("DocuSignIntegratorKey")), 
                        new KeyValuePair<string, string>("username", curCredentials.Username),
                        new KeyValuePair<string, string>("password", curCredentials.Password),
                        new KeyValuePair<string, string>("scope", "api"), 
                    }));
            try
            {
                var responseAsString = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeAnonymousType(responseAsString, new { access_token = "" });

                return responseObject.access_token;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                response.Dispose();
            }
        }

        private HttpClient CreateHttpClient(string endPoint)
        {
            return new HttpClient() { BaseAddress = new Uri(endPoint) };
        }
    }
}