using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;

namespace Fr8.TerminalBase.Services
{
    public class UpstreamQueryManager
    {
        private readonly ActivityContext _activityContext;
        private readonly IHubCommunicator _hubCommunicator;
        private Guid ActivityId => _activityContext.ActivityPayload.Id;

        public UpstreamQueryManager(ActivityContext activityContext, IHubCommunicator hubCommunicator)
        {
            _activityContext = activityContext;
            _hubCommunicator = hubCommunicator;
        }

        public async Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(CrateDirection direction)
        {
            return await _hubCommunicator.GetCratesByDirection<TManifest>(ActivityId, direction);
        }

        public async Task<List<TManifest>> GetCrateManifestsByDirection<TManifest>(CrateDirection direction)
        {
            return (await _hubCommunicator.GetCratesByDirection<TManifest>(ActivityId, direction)).Select(x => x.Content).ToList();
        }

        public async Task<List<Crate>> GetCratesByDirection(CrateDirection direction)
        {
            return await _hubCommunicator.GetCratesByDirection(ActivityId, direction);
        }
    }
}
