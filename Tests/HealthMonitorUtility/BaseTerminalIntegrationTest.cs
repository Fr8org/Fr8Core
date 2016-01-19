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
using StructureMap;

namespace HealthMonitor.Utility
{
    public abstract class BaseTerminalIntegrationTest
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

        public BaseTerminalIntegrationTest()
        {
            ObjectFactory.Initialize();
            ObjectFactory.Configure(Hub.StructureMap.StructureMapBootStrapper.LiveConfiguration);
            RestfulServiceClient = new RestfulServiceClient();
            Crate = new CrateManager();
            HMACService = new Fr8HMACService();
        }

        public abstract string TerminalName { get; }

        public string GetTerminalUrl()
        {
            return ConfigurationManager.AppSettings[TerminalName + "Url"];
        }

        public void CheckIfPayloadHasNeedsAuthenticationError(PayloadDTO payload)
        {
            var storage = Crate.GetStorage(payload);
            var operationalStateCM = storage.CrateContentsOfType<OperationalStateCM>().Single();

            Assert.AreEqual(ActionResponse.Error, operationalStateCM.CurrentActionResponse);
            Assert.AreEqual(ActionErrorCode.NO_AUTH_TOKEN_PROVIDED, operationalStateCM.CurrentActionErrorCode);
            Assert.AreEqual("No AuthToken provided.", operationalStateCM.CurrentActionErrorMessage);

        }

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

        private void AddHubCrate<T>(ActionDTO actionDTO, T crateManifest, string label, string innerLabel)
        {
            var crateStorage = Crate.GetStorage(actionDTO.ExplicitData);            

            var fullLabel = label;
            if (!string.IsNullOrEmpty(innerLabel))
            {
                fullLabel += "_" + innerLabel;
            }

            var crate = Crate<T>.FromContent(fullLabel, crateManifest);
            crateStorage.Add(crate);

            actionDTO.ExplicitData = Crate.CrateStorageAsStr(crateStorage);
        }

        public void AddCrate<T>(ActionDTO actionDTO, T crateManifest, string label)
        {
            var crateStorage = Crate.GetStorage(actionDTO.ExplicitData);            

            var crate = Crate<T>.FromContent(label, crateManifest);
            crateStorage.Add(crate);

            actionDTO.ExplicitData = Crate.CrateStorageAsStr(crateStorage);
        }

        public void AddActivityTemplate(ActionDTO actionDTO, ActivityTemplateDTO activityTemplate)
        {
            AddHubCrate(
                actionDTO,
                new StandardDesignTimeFieldsCM(
                    new FieldDTO("ActivityTemplate", JsonConvert.SerializeObject(activityTemplate))
                ),
                "HealthMonitor_ActivityTemplate",
                ""
            );
        }

        public void AddUpstreamCrate<T>(ActionDTO actionDTO, T crateManifest, string crateLabel = "")
        {
            AddHubCrate(actionDTO, crateManifest, "HealthMonitor_UpstreamCrate", crateLabel);
        }

        public void AddDownstreamCrate<T>(ActionDTO actionDTO, T crateManifest, string crateLabel = "")
        {
            AddHubCrate(actionDTO, crateManifest, "HealthMonitor_DownstreamCrate", crateLabel);
        }

        public void AddPayloadCrate<T>(ActionDTO actionDTO, T crateManifest, string crateLabel = "")
        {
            AddHubCrate(actionDTO, crateManifest, "HealthMonitor_PayloadCrate", crateLabel);
        }

        public void AddOperationalStateCrate(ActionDTO actionDTO, OperationalStateCM operationalState)
        {
            AddPayloadCrate(actionDTO, operationalState, "Operational Status");
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
