using System;
using System.Configuration;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Restful;

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
            using (var updater = Crate.UpdateStorage(actionDTO))
            {
                var fullLabel = label;
                if (!string.IsNullOrEmpty(innerLabel))
                {
                    fullLabel += "_" + innerLabel;
                }

                var crate = Crate<T>.FromContent(fullLabel, crateManifest);
                updater.CrateStorage.Add(crate);
            }
        }

        public void AddCrate<T>(ActionDTO actionDTO, T crateManifest, string label)
        {
            using (var updater = Crate.UpdateStorage(actionDTO))
            {
                var crate = Crate<T>.FromContent(label, crateManifest);
                updater.CrateStorage.Add(crate);
            }
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
