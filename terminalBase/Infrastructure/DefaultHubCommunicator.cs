using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StructureMap;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hub.Interfaces;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Utilities.Configuration.Azure;

namespace TerminalBase.Infrastructure
{
    public class DefaultHubCommunicator : IHubCommunicator
    {
        private readonly IRouteNode _routeNode;
        private readonly IRestfulServiceClient _restfulServiceClient;

        public DefaultHubCommunicator()
        {
            _routeNode = ObjectFactory.GetInstance<IRouteNode>();
            _restfulServiceClient = ObjectFactory.GetInstance<IRestfulServiceClient>();
        }

        public Task<PayloadDTO> GetProcessPayload(ActionDO actionDO, Guid containerId)
        {
            var url = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                + "api/containers/"
                + containerId.ToString("D");

            var payloadDTOTask = _restfulServiceClient
                .GetAsync<PayloadDTO>(new Uri(url, UriKind.Absolute));

            return payloadDTOTask;
        }

        public Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(
            ActionDO actionDO, CrateDirection direction)
        {
            return _routeNode.GetCratesByDirection<TManifest>(actionDO.Id, direction);
        }

        public Task<List<ActivityTemplateDTO>> GetActivityTemplates(ActivityCategory category)
        {
        }
    }
}
