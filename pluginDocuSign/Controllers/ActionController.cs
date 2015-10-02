using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Linq;
using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using PluginBase.BaseClasses;


namespace pluginDocuSign.Controllers
{    
    [RoutePrefix("actions")]
    public class ActionController : ApiController
    {
        private const string curPlugin = "pluginDocuSign";
        private BasePluginController _basePluginController = new BasePluginController();


        [HttpPost]
        [Route("configure")]
        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            return await (Task<ActionDTO>) _basePluginController
                .HandleDockyardRequest(curPlugin, "Configure", curActionDTO);
        }

        [HttpPost]
        [Route("activate")]
        public ActionDTO Activate(ActionDTO curActionDTO)
        {
            return (ActionDTO)_basePluginController.HandleDockyardRequest(curPlugin, "Activate", curActionDTO);
        }

        [HttpPost]
        [Route("deactivate")]
        public ActionDTO Deactivate(ActionDTO curActionDTO)
        {
            return (ActionDTO)_basePluginController.HandleDockyardRequest(curPlugin, "Deactivate", curActionDTO);
        }

        [HttpPost]
        [Route("authenticate")]
        public async Task<AuthTokenDTO> Authenticate(CredentialsDTO curCredentials)
        {
            // Auth sequence according to https://www.docusign.com/p/RESTAPIGuide/RESTAPIGuide.htm#OAuth2/OAuth2%20Token%20Request.htm

            var loginInfoJObject = await RunLoginInformationRequest(curCredentials);
            var baseUrl = FetchBaseUrl(loginInfoJObject);
            var oauthToken = await ObtainOAuthToken(curCredentials, baseUrl);

            return new AuthTokenDTO() { AuthToken = oauthToken };
            
        }

        private HttpClient CreateHttpClient(string endPoint)
        {
            return new HttpClient() { BaseAddress = new Uri(endPoint) };
        }

        private async Task<JObject> RunLoginInformationRequest(CredentialsDTO curCredentials)
        {
            var endPoint = ConfigurationManager.AppSettings["endpoint"];
            var loginInfoUrl = endPoint + "/login_information";

            var httpClient = CreateHttpClient(endPoint);

            var httpMessage = new HttpRequestMessage(HttpMethod.Get, "login_information");
            httpMessage.Headers.Add("X-DocuSign-Authentication", CreateDocuSignAuthHeader(curCredentials));

            var response = await httpClient.SendAsync(httpMessage);

            try
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                var resultJObject = JsonConvert.DeserializeObject<JObject>(responseContent);
                return resultJObject;
            }
            finally
            {
                if (response != null)
                {
                    response.Dispose();
                }
            }
        }

        private string CreateDocuSignAuthHeader(CredentialsDTO curCredentials)
        {
            var headerValue =
                new XElement("DocuSignCredentials",
                    new XElement("Username", curCredentials.Username),
                    new XElement("Password", curCredentials.Password),
                    new XElement("IntegratorKey", ConfigurationManager.AppSettings["DocuSignIntegratorKey"])
                )
                .ToString();

            return headerValue;
        }

        private string FetchBaseUrl(JObject loginInfoResponse)
        {
            var loginAccountsToken = loginInfoResponse.Value<JArray>("loginAccounts");

            if (loginAccountsToken == null || loginAccountsToken.Count == 0)
            {
                throw new ApplicationException("loginAccounts property was not found or empty.");
            }

            var accountToken = loginAccountsToken[0];
            var baseUrl = accountToken.Value<string>("baseUrl");

            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ApplicationException("baseUrl property was not found or empty.");
            }

            return baseUrl;
        }

        private async Task<string> ObtainOAuthToken(CredentialsDTO curCredentials, string baseUrl)
        {
            var response = await CreateHttpClient(baseUrl + "/restapi/v2/")
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
            return await (Task<PayloadDTO>)_basePluginController.HandleDockyardRequest(
                curPlugin, "Execute", actionDto);
        }
    }
}