using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Hub.Interfaces;
using Hub.Security;
using Newtonsoft.Json;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Restful;
using System.Linq;
using NUnit.Framework;
using Data.Constants;
using Data.Interfaces.DataTransferObjects.Helpers;
using StructureMap;
using System.Net.Http;

namespace HealthMonitor.Utility
{
    public abstract class BaseHubIntegrationTest
    {
        public ICrateManager _crate { get; set; }
        public IRestfulServiceClient _restfulServiceClient { get; set; }
        public IHMACService _hmacService { get; set; }
        private string terminalSecret;
        private string terminalId;
        HttpClient _httpClient;

        protected string TerminalSecret {
            get 
            {
                return terminalSecret ?? (terminalSecret = ConfigurationManager.AppSettings[TerminalName + "TerminalSecret"]);
            }
        }
        protected string TerminalId {
            get
            {
                return terminalId ?? (terminalId = ConfigurationManager.AppSettings[TerminalName + "TerminalId"]);
            }
        }
        protected string TestUserEmail = "integration_test_runner@fr8.company";
        protected string TestUserPassword = "fr8#s@lt!";

        public BaseHubIntegrationTest()
        {
            ObjectFactory.Initialize();
            ObjectFactory.Configure(Hub.StructureMap.StructureMapBootStrapper.LiveConfiguration);

            // Use a common HttpClient for all REST operations within testing session 
            // to ensure the presense of the authentication cookie. 
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = GetHubBaseUrl();

            _crate = new CrateManager();
            _hmacService = new Fr8HMACService();

            LoginUser(TestUserEmail, TestUserPassword).Wait();
            _restfulServiceClient = new RestfulServiceClient(_httpClient);
        }   
        public abstract string TerminalName { get; }


        public string GetTerminalDiscoverUrl()
        {
            return GetTerminalUrl() + "/terminals/discover";
        }

        public string GetTerminalConfigureUrl()
        {
            return GetTerminalUrl() + "/actions/configure";
        }

        public string GetTerminalActivateUrl()
        {
            return GetTerminalUrl() + "/actions/activate";
        }

        public string GetTerminalDeactivateUrl()
        {
            return GetTerminalUrl() + "/actions/deactivate";
        }

        public string GetTerminalRunUrl()
        {
            return GetTerminalUrl() + "/actions/run";
        }

        public string GetTerminalUrl()
        {
            return ConfigurationManager.AppSettings[TerminalName + "Url"];
        }

        public string GetHubApiBaseUrl()
        {
            return ConfigurationManager.AppSettings["HubApiBaseUrl"];
        }

        public void CheckIfPayloadHasNeedsAuthenticationError(PayloadDTO payload)
        {
            var storage = _crate.GetStorage(payload);
            var operationalStateCM = storage.CrateContentsOfType<OperationalStateCM>().Single();

            //extract current error message from current activity response
            ErrorDTO errorMessage;
            operationalStateCM.CurrentActivityResponse.TryParseErrorDTO(out errorMessage);

            Assert.AreEqual(ActivityResponse.Error.ToString(), operationalStateCM.CurrentActivityResponse.Type);
            Assert.AreEqual(ActionErrorCode.NO_AUTH_TOKEN_PROVIDED, operationalStateCM.CurrentActivityErrorCode);
            Assert.AreEqual("No AuthToken provided.", errorMessage.Message);
        }

        private async Task<Dictionary<string, string>> GetHMACHeader<T>(Uri requestUri, string userId, T content)
        {
            return await _hmacService.GenerateHMACHeader(requestUri, TerminalId, TerminalSecret, userId, content);
        }
        public async Task<TResponse> HttpPostAsync<TRequest, TResponse>(string url, TRequest request)
        {
            var uri = new Uri(url);
            return await _restfulServiceClient.PostAsync<TRequest, TResponse>(uri, request, null, null);
        }
        public async Task HttpDeleteAsync(string url)
        {
            var uri = new Uri(url);
            await _restfulServiceClient.DeleteAsync(uri, null, null);
        }
        public async Task<TResponse> HttpGetAsync<TResponse>(string url)
        {
            var uri = new Uri(url);
            return await _restfulServiceClient.GetAsync<TResponse>(uri);
        }

        private async Task LoginUser(string email, string password)
        {
            // Get login page and extract request validation token
            var antiFogeryToken = await GetVerificationToken(_httpClient);

            // Login user
            await Authenticate(email, password, antiFogeryToken, _httpClient);
        }

        private Uri GetHubBaseUrl()
        {
            var hubApiBaseUrl = new Uri(GetHubApiBaseUrl());
            var hubBaseUrl = new Uri(hubApiBaseUrl.Scheme + "://" + hubApiBaseUrl.Host + ":" + hubApiBaseUrl.Port);
            return hubBaseUrl;
        }

        private async Task Authenticate(string email, string password, string verificationToken, HttpClient httpClient)
        {
            var authenticationEndpointUrl = "/dockyardaccount/login";

            var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("__RequestVerificationToken", verificationToken),
                    new KeyValuePair<string, string>("Email", email),
                    new KeyValuePair<string, string>("Password", password),
                });

            var response = await httpClient.PostAsync(authenticationEndpointUrl, formContent);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
        }

        private async Task<string> GetVerificationToken(HttpClient httpClient)
        {
            var loginFormUrl = "/dockyardaccount";
            var response = await httpClient.GetAsync(loginFormUrl);
            response.EnsureSuccessStatusCode();
            var loginPageText = await response.Content.ReadAsStringAsync();
            var regEx = new System.Text.RegularExpressions.Regex(@"<input\s+name=""__RequestVerificationToken""\s+type=""hidden""\s+value=\""([\w\d-_]+)""\s+\/>");
            var matches = regEx.Match(loginPageText);
            if (matches == null || matches.Groups.Count < 2)
            {
                throw new Exception("Unable to find verification token in the login page HTML code.");
            }
            string formToken = matches.Groups[1].Value;
            return formToken;
        }

    }
}
