//using Fr8.Infrastructure.Interfaces;
//using Fr8.TerminalBase.Interfaces;
//using Fr8.TerminalBase.Services;

//namespace PlanDirectory.Infrastructure
//{
//    public class PlanDirectoryHubCommunicatorFactory : IHubCommunicatorFactory
//    {
//        private readonly string _apiUrl;
//        private readonly string _terminalToken;
//        private readonly IRestfulServiceClientFactory _factory;
//        public PlanDirectoryHubCommunicatorFactory(IRestfulServiceClientFactory factory, string apiUrl, string terminalToken)
//        {
//            _apiUrl = apiUrl;
//            _terminalToken = terminalToken;
//            _factory = factory;
//        }

//        public IHubCommunicator Create(string userId)
//        {
//            var restfulServiceClient = _factory.Create(new HubAuthenticationPDHeaderSignature(_terminalToken, userId));
//            return new DefaultHubCommunicator(restfulServiceClient, _apiUrl, _terminalToken, userId);
//        }
//    }
//}
