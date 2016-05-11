using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Data.States;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using TerminalBase.Infrastructure;

namespace TerminalBase.BaseClasses
{
    public class UpstreamQueryManager
    {
        private readonly ActivityDO _activity;
        private readonly IHubCommunicator _hubCommunicator;
        private readonly string _userId;

        public UpstreamQueryManager(ActivityDO activity, IHubCommunicator hubCommunicator, string userId)
        {
            _activity = activity;
            _hubCommunicator = hubCommunicator;
            _userId = userId;
        }

        public async Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(CrateDirection direction)
        {
            return await _hubCommunicator.GetCratesByDirection<TManifest>(_activity, direction, _userId);
        }

        public async Task<List<TManifest>> GetCrateManifestsByDirection<TManifest>(CrateDirection direction)
        {
            return (await _hubCommunicator.GetCratesByDirection<TManifest>(_activity, direction, _userId)).Select(x => x.Content).ToList();
        }

        public async Task<List<Crate>> GetCratesByDirection(CrateDirection direction)
        {
            return await _hubCommunicator.GetCratesByDirection(_activity, direction, _userId);
        }

        public async Task<FieldDescriptionsCM> GetFieldDescriptions(CrateDirection direction, AvailabilityType availability)
        {
            return await _hubCommunicator.GetDesignTimeFieldsByDirection(_activity, direction, availability, _userId);
        }

        public async Task<Crate<FieldDescriptionsCM>> GetFieldDescriptionsCrate(string label, AvailabilityType availability)
        {
            var curUpstreamFields = await _hubCommunicator.GetDesignTimeFieldsByDirection(_activity, CrateDirection.Upstream, availability, _userId);
            return Crate<FieldDescriptionsCM>.FromContent(label, curUpstreamFields);
        }

        public async Task<List<CrateManifestType>> GetUpstreamManifestList()
        {
            var upstreamCrates = await GetCratesByDirection<Manifest>(CrateDirection.Upstream);
            return upstreamCrates.Where(x => !BaseTerminalActivity.ExcludedManifestTypes.Contains(x.ManifestType)).Select(f => f.ManifestType).Distinct().ToList();
        }

        public async Task<Crate<FieldDescriptionsCM>> GetUpstreamManifestListCrate(string label = "AvailableUpstreamManifests")
        {
            var manifestList = await GetUpstreamManifestList();
            var fields = manifestList.Select(f => new FieldDTO(f.Type, f.Id.ToString())).ToArray();

            return Crate<FieldDescriptionsCM>.FromContent(label, new FieldDescriptionsCM(fields));
        }

        public async Task<List<string>> GetUpstreamCrateLabelList()
        {
            var curCrates = await this.GetCratesByDirection<Manifest>(CrateDirection.Upstream);
            return curCrates.Where(x => !BaseTerminalActivity.ExcludedManifestTypes.Contains(x.ManifestType)).Select(f => f.Label).Distinct().ToList();
        }
        
        public async Task<Crate<FieldDescriptionsCM>> GetUpstreamCrateLabelListCrate(string label = "AvailableUpstreamLabels")
        {
            var labelList = await GetUpstreamCrateLabelList();
            var fields = labelList.Select(f => new FieldDTO(f, f)).ToArray();

            return Crate<FieldDescriptionsCM>.FromContent(label, new FieldDescriptionsCM (fields));
        }
    }
}
