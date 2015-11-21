using System;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;
using Hub.Managers.APIManagers.Transmitters.Restful;
using StructureMap;
using Utilities.Configuration.Azure;

namespace TerminalBase.Infrastructure
{
    public class DefaultHubCommunicator : IHubCommunicator
    {
        private readonly IRestfulServiceClient _restfulServiceClient;

        public DefaultHubCommunicator()
        {
            _restfulServiceClient = ObjectFactory.GetInstance<IRestfulServiceClient>();
        }

        public async Task<PayloadDTO> GetProcessPayload(Guid containerId)
        {
            var url = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                + "api/containers/"
                + containerId.ToString("D");

            var payloadDTO = await _restfulServiceClient
                .GetAsync<PayloadDTO>(new Uri(url, UriKind.Absolute));

            return payloadDTO;
        }
    }
}
