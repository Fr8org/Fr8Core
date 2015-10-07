using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using terminal_base.BaseClasses;
using terminal_DocuSign.DataTransferObjects;


namespace terminal_DocuSign.Controllers
{    
    [RoutePrefix("actions")]
    public class ActionController : ApiController
    {
        private const string _curTerminal = "terminal_DocuSign";
        private BaseTerminalController _baseTerminalController = new BaseTerminalController();


        [HttpPost]
        [Route("configure")]
        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            return await (Task<ActionDTO>) _baseTerminalController
                .HandleDockyardRequest(_curTerminal, "Configure", curActionDTO);
        }

        [HttpPost]
        [Route("activate")]
        public ActionDTO Activate(ActionDTO curActionDTO)
        {
            return (ActionDTO)_baseTerminalController.HandleDockyardRequest(_curTerminal, "Activate", curActionDTO);
        }

        [HttpPost]
        [Route("deactivate")]
        public ActionDTO Deactivate(ActionDTO curActionDTO)
        {
            return (ActionDTO)_baseTerminalController.HandleDockyardRequest(_curTerminal, "Deactivate", curActionDTO);
        }

        [HttpPost]
        [Route("authenticate")]
        public async Task<AuthTokenDTO> Authenticate(CredentialsDTO curCredentials)
        {
            // Auth sequence according to https://www.docusign.com/p/RESTAPIGuide/RESTAPIGuide.htm#OAuth2/OAuth2%20Token%20Request.htm

            var oauthToken = await ObtainOAuthToken(curCredentials, ConfigurationManager.AppSettings["endpoint"]);

            var docuSignAuthDTO = new DocuSignAuthDTO()
            {
                Email = curCredentials.Username,
                ApiPassword = oauthToken
            };

            return new AuthTokenDTO()
            {
                Token = JsonConvert.SerializeObject(docuSignAuthDTO),
                ExternalAccountId = curCredentials.Username
            };
        }

        private HttpClient CreateHttpClient(string endPoint)
        {
            return new HttpClient() { BaseAddress = new Uri(endPoint) };
        }

        private async Task<string> ObtainOAuthToken(CredentialsDTO curCredentials, string baseUrl)
        {
            var response = await CreateHttpClient(baseUrl)
                .PostAsync("oauth2/token",
                    new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("grant_type", "password"), 
                        new KeyValuePair<string, string>("client_id", ConfigurationManager.AppSettings["DocuSignIntegratorKey"]), 
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
            finally
            {
                response.Dispose();
            }
        }

        [HttpPost]
        [Route("execute")]
        public async Task<PayloadDTO> Execute(ActionDTO actionDto)
        {
            return await (Task<PayloadDTO>)_baseTerminalController.HandleDockyardRequest(
                _curTerminal, "Execute", actionDto);
        }
    }
}