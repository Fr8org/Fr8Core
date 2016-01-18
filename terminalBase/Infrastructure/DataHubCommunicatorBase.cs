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
using Data.Constants;

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

        public Task<PayloadDTO> GetPayload(ActionDO actionDO, Guid containerId, string userId)
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

        public Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(ActionDO actionDO, CrateDirection direction, string userId)
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

        public Task<List<Crate>> GetCratesByDirection(ActionDO actionDO, CrateDirection direction, string userId)
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

        public async Task CreateAlarm(AlarmDTO alarmDTO, string userId)
        {

        }

        public Task<List<ActivityTemplateDTO>> GetActivityTemplates(ActionDO actionDO, string userId)
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

        public async Task<List<ActivityTemplateDTO>> GetActivityTemplates(ActionDO actionDO, ActivityCategory category, string userId)
        {
            var allTemplates = await GetActivityTemplates(actionDO, userId);
            var activityTemplates = allTemplates
                .Where(x => x.Category == category)
                .ToList();

            return activityTemplates;
        }

        public async Task<List<ActivityTemplateDTO>> GetActivityTemplates(ActionDO actionDO, string tag, string userId)
        {
            var allTemplates = await GetActivityTemplates(actionDO, userId);
            if (string.IsNullOrEmpty(tag))
            {
                return allTemplates;
            }

            var activityTemplates = allTemplates
                .Where(x => x.Tags != null && x.Tags.Split(',').Any(y => y.Equals(tag, StringComparison.InvariantCultureIgnoreCase)))
                .ToList();

            return activityTemplates;
        }

        public Task<List<FieldValidationResult>> ValidateFields(List<FieldValidationDTO> fields, string userId)
        {
            return Task.FromResult(new List<FieldValidationResult>());
        }

        public async Task<StandardDesignTimeFieldsCM> GetDesignTimeFieldsByDirection(ActionDO actionDO, CrateDirection direction, AvailabilityType availability, string userId)
        {
            //This code only supports integration testing scenarios

            StandardDesignTimeFieldsCM mergedFields = new StandardDesignTimeFieldsCM();
            var curCrates = await GetCratesByDirection<StandardDesignTimeFieldsCM>(actionDO, direction, userId);
            mergedFields.Fields.AddRange(Crate.MergeContentFields(curCrates).Fields);
            return mergedFields;
        }

        public Task<StandardDesignTimeFieldsCM> GetDesignTimeFieldsByDirection(Guid actionId, CrateDirection direction, AvailabilityType availability, string userId)
        {
            throw new NotImplementedException();
        }
    }
}
