using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates.Helpers;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.TerminalBase.Services
{
    public class HubLoggerService : IHubLoggerService
    {
        private readonly IHubDiscoveryService _hubDiscoveryService;
        private readonly IActivityStore _activityStore;

        public HubLoggerService(IHubDiscoveryService hubDiscoveryService, IActivityStore activityStore)
        {
            _hubDiscoveryService = hubDiscoveryService;
            _activityStore = activityStore;
        }
        
        public async Task Log(LoggingDataCM data)
        {
            data.ObjectId = _activityStore.Terminal.Name;

            var hubCommunicator = await _hubDiscoveryService.GetMasterHubCommunicator();
            
            await hubCommunicator.SendEvent(LoggingDataCrateFactory.Create(data));
        }
    }
}
