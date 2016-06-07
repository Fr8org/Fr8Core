using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using fr8.Infrastructure.Data.Crates;
using fr8.Infrastructure.Data.States;
using TerminalBase.Infrastructure;
using TerminalBase.Models;

namespace TerminalBase.Services
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
