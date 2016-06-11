using System.Configuration;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.Interfaces;

namespace Fr8.TerminalBase.Services
{
    public class HubEventReporter : IHubEventReporter
    {
        private readonly IRestfulServiceClient _restfulServiceClient;
        private readonly IActivityStore _activityStore;
        private readonly IHMACService _hmacService;
        private readonly string _apiUrl;

        public TerminalDTO Terminal => _activityStore.Terminal;

        public HubEventReporter(IRestfulServiceClient restfulServiceClient, IActivityStore activityStore, IHMACService hmacService)
        {
            _restfulServiceClient = restfulServiceClient;
            _activityStore = activityStore;
            _hmacService = hmacService;

            _apiUrl = $"{CloudConfigurationManager.GetSetting("CoreWebServerUrl")}api/{CloudConfigurationManager.GetSetting("HubApiVersion")}";
        }

        public async Task Broadcast(Crate eventPayload)
        {
            var masterCommunicator = await GetMasterHubCommunicator();

            await masterCommunicator.SendEvent(eventPayload);
        }

        public Task<IHubCommunicator> GetMasterHubCommunicator()
        {
            var secret = CloudConfigurationManager.GetSetting("TerminalSecret") ?? ConfigurationManager.AppSettings[_activityStore.Terminal.Name + "TerminalSecret"];
            var terminalId = CloudConfigurationManager.GetSetting("TerminalId") ?? ConfigurationManager.AppSettings[_activityStore.Terminal.Name + "TerminalId"];

            return Task.FromResult<IHubCommunicator>(new DefaultHubCommunicator(_restfulServiceClient, _hmacService, _apiUrl, terminalId, secret));
        }
    }
}
