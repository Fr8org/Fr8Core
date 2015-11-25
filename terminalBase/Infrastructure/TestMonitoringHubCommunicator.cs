using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hub.Managers;
using StructureMap;

namespace TerminalBase.Infrastructure
{
    public class TestMonitoringHubCommunicator : IHubCommunicator
    {
        public ICrateManager Crate { get; set; }

        public TestMonitoringHubCommunicator()
        {
            Crate = ObjectFactory.GetInstance<ICrateManager>();
        }

        private void StripLabelPrefix(IEnumerable<Crate> crates, string prefix)
        {
            foreach (var crate in crates)
            {
                if (crate.Label != prefix && crate.Label.StartsWith(prefix + "_"))
                {
                    crate.Label = crate.Label.Substring((prefix + "_").Length);
                }
            }
        }

        public Task<PayloadDTO> GetProcessPayload(ActionDO actionDO, Guid containerId)
        {
            var payload = new PayloadDTO(containerId)
            {
                CrateStorage = new CrateStorageDTO()
            };

            var crateStorage = Crate.GetStorage(actionDO);
            using (var updater = Crate.UpdateStorage(payload))
            {
                var crates = crateStorage
                    .Where(x => x.Label.StartsWith("HealthMonitor_PayloadCrate"))
                    .ToList();

                StripLabelPrefix(crates, "HealthMonitor_PayloadCrate");

                updater.CrateStorage.AddRange(crates);
            }

            return Task.FromResult(payload);
        }

        public Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(
            ActionDO actionDO, CrateDirection direction)
        {
            var searchLabel = direction == CrateDirection.Upstream
                ? "HealthMonitor_UpstreamCrate"
                : "HealthMonitor_DownstreamCrate";

            var crateStorage = Crate.GetStorage(actionDO);
            var crates = crateStorage
                .CratesOfType<TManifest>(x => x.Label.StartsWith(searchLabel))
                .ToList();

            StripLabelPrefix(crates, searchLabel);

            return Task.FromResult(crates);
        }
    }
}
