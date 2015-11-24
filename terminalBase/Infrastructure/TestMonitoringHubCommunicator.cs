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

        public Task<PayloadDTO> GetProcessPayload(ActionDO actionDO, Guid containerId)
        {
            var payload = new PayloadDTO(containerId)
            {
                CrateStorage = new CrateStorageDTO()
            };

            var crateStorage = Crate.GetStorage(actionDO);
            using (var updater = Crate.UpdateStorage(payload))
            {
                updater.CrateStorage.AddRange(crateStorage.Where(x => x.Label == "HealthMonitor_PayloadCrate"));
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
            var crates = crateStorage.CratesOfType<TManifest>(x => x.Label == searchLabel).ToList();

            return Task.FromResult(crates);
        }
    }
}
