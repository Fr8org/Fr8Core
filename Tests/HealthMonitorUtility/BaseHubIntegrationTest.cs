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

namespace HealthMonitor.Utility
{
    public abstract class BaseHubIntegrationTest
    {
        public ICrateManager Crate { get; set; }
        public IRestfulServiceClient RestfulServiceClient { get; set; }
        public IHMACService HMACService { get; set; }
        private string terminalSecret;
        private string terminalId;
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
            RestfulServiceClient = new RestfulServiceClient();
            Crate = new CrateManager();
            HMACService = new Fr8HMACService();
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
            var storage = Crate.GetStorage(payload);
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
            return await HMACService.GenerateHMACHeader(requestUri, TerminalId, TerminalSecret, userId, content);
        }
        public async Task<TResponse> HttpPostAsync<TRequest, TResponse>(string url, TRequest request)
        {
            var uri = new Uri(url);
            return await RestfulServiceClient.PostAsync<TRequest, TResponse>(uri, request, null, await GetHMACHeader(uri, "testUser", request));
        }
        public async Task<TResponse> HttpGetAsync<TResponse>(string url)
        {
            var uri = new Uri(url);
            return await RestfulServiceClient.GetAsync<TResponse>(uri);
        }
    }
}
