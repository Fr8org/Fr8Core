using System;
using System.Collections.Generic;
using System.Linq;
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

        public Task<PayloadDTO> GetPayload(ActionDO actionDO, Guid containerId)
        {
            var url = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                + "api/" + CloudConfigurationManager.GetSetting("HubApiVersion") + "/containers?id="
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

        public Task<List<Crate>> GetCratesByDirection(ActionDO actionDO, CrateDirection direction)
        {
            return _routeNode.GetCratesByDirection(actionDO.Id, direction);
        }

        public async Task CreateAlarm(AlarmDTO alarmDTO)
        {
            var hubAlarmsUrl = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                + "api/" + CloudConfigurationManager.GetSetting("HubApiVersion") + "/alarms";

            await _restfulServiceClient.PostAsync(new Uri(hubAlarmsUrl), alarmDTO);
        }

        public async Task<List<ActivityTemplateDTO>> GetActivityTemplates(ActionDO actionDO)
        {
            var hubUrl = CloudConfigurationManager.GetSetting("CoreWebServerUrl") 
                + "api/" + CloudConfigurationManager.GetSetting("HubApiVersion") + "/routenodes/available";

            var allCategories = await _restfulServiceClient
                .GetAsync<IEnumerable<ActivityTemplateCategoryDTO>>(new Uri(hubUrl));

            var templates = allCategories.SelectMany(x => x.Activities);
            return templates.ToList();
        }

        public async Task<List<ActivityTemplateDTO>> GetActivityTemplates(
            ActionDO actionDO, ActivityCategory category)
        {
            var allTemplates = await GetActivityTemplates(actionDO);
            var templates = allTemplates.Where(x => x.Category == category);

            return templates.ToList();
        }

        public async Task<List<ActivityTemplateDTO>> GetActivityTemplates(
            ActionDO actionDO, string tag)
        {
            var hubUrl = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                + "api/" + CloudConfigurationManager.GetSetting("HubApiVersion") + "/routenodes/available?tag=";

            if (string.IsNullOrEmpty(tag))
            {
                hubUrl += "[all]";
            }
            else
            {
                hubUrl += tag;
            }

            var templates = await _restfulServiceClient
                .GetAsync<List<ActivityTemplateDTO>>(new Uri(hubUrl));

            return templates;
        }
    }
}
