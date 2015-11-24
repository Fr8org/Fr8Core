using System.Configuration;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Hub.Managers;

namespace HealthMonitor.Utility
{
    public abstract class BaseHealthMonitorTest
    {
        public JsonRestClient JsonRestClient { get; set; }
        public ICrateManager Crate { get; set; }

        public BaseHealthMonitorTest()
        {
            JsonRestClient = new JsonRestClient();
            Crate = new CrateManager();
        }

        public abstract string TerminalName { get; }

        public string GetTerminalUrl()
        {
            return ConfigurationManager.AppSettings[TerminalName + "Url"];
        }

        public string GetTerminalConfigureUrl()
        {
            return GetTerminalUrl() + "/actions/configure";
        }

        private void AddCrate<T>(ActionDTO actionDTO, T crateManifest, string label)
        {
            using (var updater = Crate.UpdateStorage(actionDTO))
            {
                var crate = Crate<T>.FromContent("HealthMonitor_UpstreamCrate", crateManifest);
                updater.CrateStorage.Add(crate);
            }
        }

        public void AddUpstreamCrate<T>(ActionDTO actionDTO, T crateManifest)
        {
            AddCrate(actionDTO, crateManifest, "HealthMonitor_UpstreamCrate");
        }

        public void AddDownstreamCrate<T>(ActionDTO actionDTO, T crateManifest)
        {
            AddCrate(actionDTO, crateManifest, "HealthMonitor_DownstreamCrate");
        }

        public void AddPayloadCrate<T>(ActionDTO actionDTO, T crateManifest)
        {
            AddCrate(actionDTO, crateManifest, "HealthMonitor_PayloadCrate");
        }
    }
}
