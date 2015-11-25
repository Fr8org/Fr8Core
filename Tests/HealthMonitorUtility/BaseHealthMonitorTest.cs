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

        private void AddCrate<T>(ActionDTO actionDTO, T crateManifest, string label, string innerLabel)
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

        public void AddUpstreamCrate<T>(ActionDTO actionDTO, T crateManifest, string crateLabel = "")
        {
            AddCrate(actionDTO, crateManifest, "HealthMonitor_UpstreamCrate", crateLabel);
        }

        public void AddDownstreamCrate<T>(ActionDTO actionDTO, T crateManifest, string crateLabel = "")
        {
            AddCrate(actionDTO, crateManifest, "HealthMonitor_DownstreamCrate", crateLabel);
        }

        public void AddPayloadCrate<T>(ActionDTO actionDTO, T crateManifest, string crateLabel = "")
        {
            AddCrate(actionDTO, crateManifest, "HealthMonitor_PayloadCrate", crateLabel);
        }
    }
}
