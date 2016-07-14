using Fr8.Infrastructure.Interfaces;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Services;
using PlanDirectory.Interfaces;

namespace PlanDirectory.Infrastructure
{
    public class DefaultHubCommunicatorFactory : IHubCommunicatorFactory
    {
        private readonly string _apiUrl;
        private readonly string _terminalToken;
        private readonly IRestfulServiceClientFactory _factory;
        public DefaultHubCommunicatorFactory(IRestfulServiceClientFactory factory, string apiUrl, string terminalToken)
        {
            _apiUrl = apiUrl;
            _terminalToken = terminalToken;
            _factory = factory;
        }

        public IHubCommunicator Create(string userId)
        {
            var restfulServiceClient = _factory.Create();
            return new PlanDirectoryHubCommunicator(restfulServiceClient, _apiUrl, _terminalToken, userId);
        }
    }
}
