using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StructureMap;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;

namespace TerminalBase.Infrastructure
{
    public abstract class DataHubCommunicatorBase : IHubCommunicator
    {
        public ICrateManager Crate { get; set; }

        protected DataHubCommunicatorBase()
        {
            Crate = ObjectFactory.GetInstance<ICrateManager>();
        }

        protected abstract string LabelPrefix { get; }

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

            var crateStorage = Crate.GetStorage(actionDO.ExplicitData);
            using (var updater = Crate.UpdateStorage(payload))
            {
                var crates = crateStorage
                    .Where(x => x.Label.StartsWith(LabelPrefix + "_PayloadCrate"))
                    .ToList();

                StripLabelPrefix(crates, LabelPrefix + "_PayloadCrate");

                updater.CrateStorage.AddRange(crates);
            }

            return Task.FromResult(payload);
        }

        public Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(
            ActionDO actionDO, CrateDirection direction)
        {
            var searchLabel = direction == CrateDirection.Upstream
                ? LabelPrefix + "_UpstreamCrate"
                : LabelPrefix + "_DownstreamCrate";

            var crateStorage = Crate.GetStorage(actionDO.ExplicitData);
            var crates = crateStorage
                .CratesOfType<TManifest>(x => x.Label.StartsWith(searchLabel))
                .ToList();

            StripLabelPrefix(crates, searchLabel);

            return Task.FromResult(crates);
        }

        public Task<List<Crate>> GetCratesByDirection(ActionDO actionDO, CrateDirection direction)
        {
            var searchLabel = direction == CrateDirection.Upstream
                ? LabelPrefix + "_UpstreamCrate"
                : LabelPrefix + "_DownstreamCrate";

            var crateStorage = Crate.GetStorage(actionDO);
            var crates = crateStorage
                .Where(x => x.Label.StartsWith(searchLabel))
                .ToList();

            StripLabelPrefix(crates, searchLabel);

            return Task.FromResult(crates);
        }

        //TODO create this function
        public Task CreateAlarm(AlarmDTO alarmDTO)
        {
            throw new NotImplementedException();
        }

        public Task<List<ActivityTemplateDTO>> GetActivityTemplates(ActionDO actionDO)
        {
            var searchLabel = LabelPrefix + "_ActivityTemplate";

            var crateStorage = Crate.GetStorage(actionDO.ExplicitData);
            var activityTemplates = crateStorage
                .Where(x => x.Label == searchLabel)
                .Select(x => JsonConvert.DeserializeObject<ActivityTemplateDTO>(
                    x.Get<StandardDesignTimeFieldsCM>().Fields[0].Value
                    )
                )
                .ToList();

            return Task.FromResult(activityTemplates);
        }

        public async Task<List<ActivityTemplateDTO>> GetActivityTemplates(
            ActionDO actionDO, ActivityCategory category)
        {
            var allTemplates = await GetActivityTemplates(actionDO);
            var activityTemplates = allTemplates
                .Where(x => x.Category == category)
                .ToList();

            return activityTemplates;
        }

        public async Task<List<ActivityTemplateDTO>> GetActivityTemplates(
            ActionDO actionDO, string tag)
        {
            var allTemplates = await GetActivityTemplates(actionDO);
            if (string.IsNullOrEmpty(tag))
            {
                return allTemplates;
            }

            var activityTemplates = allTemplates
                .Where(x => x.Tags != null && x.Tags.Split(',').Any(y => y.Equals(tag, StringComparison.InvariantCultureIgnoreCase)))
                .ToList();

            return activityTemplates;
        }
    }
}
