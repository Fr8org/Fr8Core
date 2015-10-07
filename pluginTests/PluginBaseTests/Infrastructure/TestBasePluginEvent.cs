using Core.Managers.APIManagers.Transmitters.Restful;
using Moq;
using TerminalBase.Infrastructure;

namespace terminalTests.TerminalBaseTests.Infrastructure
{
    internal class TestBaseTerminalEvent : BaseTerminalEvent
    {
        private IRestfulServiceClient _restfulServiceClient;

        internal IRestfulServiceClient RestfulServiceClient
        {
            get { return _restfulServiceClient; }
            private set { _restfulServiceClient = value; }
        }
        protected override IRestfulServiceClient PrepareRestClient()
        {
            if (_restfulServiceClient == null)
            {
                _restfulServiceClient = new Mock<IRestfulServiceClient>(MockBehavior.Default).Object;
            }

            return _restfulServiceClient;
        }
    }
}
