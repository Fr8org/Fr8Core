using System;
using System.Configuration;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Restful;
using System.Linq;
using NUnit.Framework;
using Data.Constants;

namespace HealthMonitor.Utility
{
    public abstract class BaseHealthMonitorTest
    {
        public ICrateManager Crate { get; set; }
        public IRestfulServiceClient RestfulServiceClient { get; set; }

        public BaseHealthMonitorTest()
        {
            RestfulServiceClient = new RestfulServiceClient();
            Crate = new CrateManager();
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

        public async Task<TResponse> HttpPostAsync<TRequest, TResponse>(string url, TRequest request)
        {
            return await RestfulServiceClient.PostAsync<TRequest, TResponse>(new Uri(url), request);
        }

        public async Task<TResponse> HttpGetAsync<TResponse>(string url)
        {
            return await RestfulServiceClient.GetAsync<TResponse>(new Uri(url));
        }
    }
}
