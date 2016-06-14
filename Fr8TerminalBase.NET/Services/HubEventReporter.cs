using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Interfaces;

namespace Fr8.TerminalBase.Services
{
    public class HubEventReporter : IHubEventReporter
    {
        private readonly IHubDiscoveryService _hubDiscovery;
        private readonly IActivityStore _activityStore;

        public TerminalDTO Terminal => _activityStore.Terminal;

        public HubEventReporter(IHubDiscoveryService hubDiscovery, IActivityStore activityStore)
        {
            _hubDiscovery = hubDiscovery;
            _activityStore = activityStore;
        }

        public async Task Broadcast(Crate eventPayload)
        {
            var masterCommunicator = await GetMasterHubCommunicator();

            await masterCommunicator.SendEvent(eventPayload);
        }

        public Task<IHubCommunicator> GetMasterHubCommunicator()
        {
            return _hubDiscovery.GetMasterHubCommunicator();
        }
    }
}
