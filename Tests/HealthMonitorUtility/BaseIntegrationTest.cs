using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Restful;
using System.Linq;
using NUnit.Framework;
using StructureMap;
using System.Net.Http;
using Data.Interfaces;
using Fr8Data.Constants;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.DataTransferObjects.Helpers;
using Fr8Data.Manifests;

namespace HealthMonitor.Utility
{
    public abstract class BaseIntegrationTest
    {
        public ICrateManager Crate { get; set; }
        public IRestfulServiceClient RestfulServiceClient { get; set; }
        public IHMACService _hmacService { get; set; }
        private string _terminalSecret;
        private string _terminalId;
        HttpClient _httpClient;
        protected string _baseUrl;
        protected int currentTerminalVersion = 1;
        protected string _terminalUrl;

        protected string TerminalSecret
        {
            get
            {
                return _terminalSecret ?? (_terminalSecret = ConfigurationManager.AppSettings[TerminalName + "TerminalSecret"]);
            }
        }
        protected string TerminalId
        {
            get
            {
                return _terminalId ?? (_terminalId = ConfigurationManager.AppSettings[TerminalName + "TerminalId"]);
            }
        }
        protected string TerminalUrl
        {
            get
            {
                return _terminalUrl ?? (_terminalUrl = GetTerminalUrlInternally());
            }
        }

        public string GetTerminalEventsUrl()
        {
            return TerminalUrl + $"/terminals/{TerminalName}/events";
        }
        
        public BaseIntegrationTest()
        {
            RestfulServiceClient = new RestfulServiceClient();
            Crate = new CrateManager();
        }

        private string GetTerminalUrlInternally()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var terminal = uow.TerminalRepository.GetQuery()
                    .FirstOrDefault(t => t.Version == currentTerminalVersion.ToString() && t.Name == TerminalName);
                if (null == terminal)
                {
                    throw new Exception(
                        String.Format("Terminal with name {0} and version {1} not found", TerminalName, currentTerminalVersion));
                }
                return Utilities.NormalizeSchema(terminal.Endpoint);
            }
        }

        public abstract string TerminalName { get; }


        public string GetTerminalDiscoverUrl()
        {
            return TerminalUrl + "/terminals/discover";
        }

        public string GetTerminalConfigureUrl()
        {
            return TerminalUrl + "/activities/configure";
        }

        public string GetTerminalActivateUrl()
        {
            return TerminalUrl + "/activities/activate";
        }

        public string GetTerminalDeactivateUrl()
        {
            return TerminalUrl + "/activities/deactivate";
        }

        public string GetTerminalRunUrl()
        {
            return TerminalUrl + "/activities/run";
        }

        public string GetTerminalUrl()
        {
            return TerminalUrl;
        }

        protected void AddHubCrate<T>(Fr8DataDTO dataDTO, T crateManifest, string label, string innerLabel)
        {
            var crateStorage = Crate.GetStorage(dataDTO.ExplicitData);

            var fullLabel = label;
            if (!string.IsNullOrEmpty(innerLabel))
            {
                fullLabel += "_" + innerLabel;
            }

            var crate = Crate<T>.FromContent(fullLabel, crateManifest);
            crateStorage.Add(crate);

            dataDTO.ExplicitData = Crate.CrateStorageAsStr(crateStorage);
        }

        public void AddPayloadCrate<T>(Fr8DataDTO dataDTO, T crateManifest, string crateLabel = "")
        {
            AddHubCrate(dataDTO, crateManifest, "HealthMonitor_PayloadCrate", crateLabel);
        }

        public void AddOperationalStateCrate(Fr8DataDTO dataDTO, OperationalStateCM operationalState)
        {
            AddPayloadCrate(dataDTO, operationalState, "Operational Status");
        }

        public void CheckIfPayloadHasNeedsAuthenticationError(PayloadDTO payload)
        {
            var storage = Crate.GetStorage(payload);
            var operationalStateCM = storage.CrateContentsOfType<OperationalStateCM>().Single();

            //extract current error message from current activity response
            ErrorDTO errorMessage;
            operationalStateCM.CurrentActivityResponse.TryParseErrorDTO(out errorMessage);

            Assert.AreEqual(ActivityResponse.Error.ToString(), operationalStateCM.CurrentActivityResponse.Type);
            Assert.AreEqual(ActivityErrorCode.AUTH_TOKEN_NOT_PROVIDED_OR_INVALID, operationalStateCM.CurrentActivityErrorCode);
            Assert.AreEqual("No AuthToken provided.", errorMessage.Message);
        }

        private async Task<Dictionary<string, string>> GetHMACHeader<T>(Uri requestUri, string userId, T content)
        {
            return await _hmacService.GenerateHMACHeader(requestUri, TerminalId, TerminalSecret, userId, content);
        }
        public async Task<TResponse> HttpPostAsync<TRequest, TResponse>(string url, TRequest request)
        {
            var uri = new Uri(url);
            return await RestfulServiceClient.PostAsync<TRequest, TResponse>(uri, request, null, null);
        }

        public async Task<TResponse> HttpPostAsync<TResponse>(string url, HttpContent content)
        {
            var uri = new Uri(url);
            return await RestfulServiceClient.PostAsync<TResponse>(uri, content, null, null);
        }
        public async Task HttpDeleteAsync(string url)
        {
            var uri = new Uri(url);
            await RestfulServiceClient.DeleteAsync(uri, null, null);
        }
        public async Task<TResponse> HttpGetAsync<TResponse>(string url)
        {
            var uri = new Uri(url);
            return await RestfulServiceClient.GetAsync<TResponse>(uri);
        }
    }
}
