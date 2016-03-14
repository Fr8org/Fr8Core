using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using TerminalBase.Infrastructure;

namespace TerminalBase.BaseClasses
{
    public class UpstreamNavigator
    {
        private readonly ActivityDO _activity;
        private readonly IHubCommunicator _hubCommunicator;
        private readonly string _userId;

        public UpstreamNavigator(ActivityDO activity, IHubCommunicator hubCommunicator, string userId)
        {
            _activity = activity;
            _hubCommunicator = hubCommunicator;
            _userId = userId;
        }
        
        public virtual async Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(CrateDirection direction)
        {
            return await _hubCommunicator.GetCratesByDirection<TManifest>(_activity, direction, _userId);
        }

        public virtual async Task<List<Crate>> GetCratesByDirection(CrateDirection direction)
        {
            return await _hubCommunicator.GetCratesByDirection(_activity, direction, _userId);
        }

        public virtual async Task<FieldDescriptionsCM> GetDesignTimeFields(CrateDirection direction, AvailabilityType availability = AvailabilityType.NotSet)
        {
            var mergedFields = await _hubCommunicator.GetDesignTimeFieldsByDirection(_activity.Id, direction, availability, _userId);
            return mergedFields;
        }
        
        public virtual async Task<List<CrateManifestType>> BuildUpstreamManifestList()
        {
            var upstreamCrates = await GetCratesByDirection<Manifest>(CrateDirection.Upstream);
            return upstreamCrates.Where(x => !BaseTerminalActivity.ExcludedManifestTypes.Contains(x.ManifestType)).Select(f => f.ManifestType).Distinct().ToList();
        }

        public virtual async Task<List<string>> BuildUpstreamCrateLabelList()
        {
            var curCrates = await this.GetCratesByDirection<Manifest>(CrateDirection.Upstream);
            return curCrates.Where(x => !BaseTerminalActivity.ExcludedManifestTypes.Contains(x.ManifestType)).Select(f => f.Label).Distinct().ToList();
        }

        public virtual async Task<Crate<FieldDescriptionsCM>> GetUpstreamManifestListCrate()
        {
            var manifestList = await BuildUpstreamManifestList();
            var fields = manifestList.Select(f => new FieldDTO(f.Type, f.Id.ToString())).ToArray();

            return Crate<FieldDescriptionsCM>.FromContent("AvailableUpstreamManifests", new FieldDescriptionsCM {Fields = fields.ToList()});
        }

        public virtual async Task<Crate<FieldDescriptionsCM>> GetUpstreamCrateLabelListCrate()
        {
            var labelList = await BuildUpstreamCrateLabelList();
            var fields = labelList.Select(f => new FieldDTO(f, f)).ToArray();

            return Crate<FieldDescriptionsCM>.FromContent("AvailableUpstreamLabels", new FieldDescriptionsCM { Fields = fields.ToList() });
        }

        protected virtual async Task<List<Crate<StandardFileDescriptionCM>>> GetUpstreamFileHandleCrates()
        {
            return await _hubCommunicator.GetCratesByDirection<StandardFileDescriptionCM>(_activity, CrateDirection.Upstream, _userId);
        }

        protected async Task<Crate<FieldDescriptionsCM>> MergeUpstreamFields(string label)
        {
            var curUpstreamFields = (await GetDesignTimeFields(CrateDirection.Upstream)).Fields.ToArray();
            return Crate<FieldDescriptionsCM>.FromContent(label, new FieldDescriptionsCM {Fields = curUpstreamFields.ToList()});
        }

        /// <summary>
        /// Creates a crate with available design-time fields.
        /// </summary>
        /// <param name="activityDO">ActionDO.</param>
        /// <returns></returns>
        protected async Task<Crate> CreateAvailableFieldsCrate(string crateLabel = "Upstream Terminal-Provided Fields",
                                                               AvailabilityType availabilityTypeUpstream = AvailabilityType.RunTime,
                                                               AvailabilityType availabilityTypeFieldsCrate = AvailabilityType.Configuration)
        {
            var curUpstreamFields = await _hubCommunicator.GetDesignTimeFieldsByDirection(_activity, CrateDirection.Upstream, availabilityTypeUpstream, _userId);

            if (curUpstreamFields == null)
            {
                curUpstreamFields = new FieldDescriptionsCM();
            }

            return Crate<FieldDescriptionsCM>.FromContent(crateLabel, new FieldDescriptionsCM() { Fields = curUpstreamFields.Fields }, availabilityTypeFieldsCrate);
        }
    }
}
