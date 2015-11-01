using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using StructureMap;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Hub.Services;

namespace Hub.Managers.APIManagers.Transmitters.Plugin
{
    public class PluginTransmitter : RestfulServiceClient, IPluginTransmitter
    {
        /// <summary>
        /// Posts ActionDTO to "/actions/&lt;actionType&gt;"
        /// </summary>
        /// <param name="curActionType">Action Type</param>
        /// <param name="actionDTO">DTO</param>
        /// <remarks>Uses <paramref name="curActionType"/> argument for constructing request uri replacing all space characters with "_"</remarks>
        /// <returns></returns>
        public async Task<TResponse> CallActionAsync<TResponse>(string curActionType, ActionDTO actionDTO)
        {
            if (actionDTO == null)
            {
                throw new ArgumentNullException("actionDTO");
            }

            if ((actionDTO.ActivityTemplateId  == null || actionDTO.ActivityTemplateId == 0) && actionDTO.ActivityTemplate == null)
            {
                throw new ArgumentOutOfRangeException("actionDTO", actionDTO.ActivityTemplateId, "ActivityTemplate must be specified either explicitly or by using ActivityTemplateId");
            }

            int pluginId;

            if (actionDTO.ActivityTemplate == null)
            {
                var activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>().GetByKey(actionDTO.ActivityTemplateId.Value);
                actionDTO.ActivityTemplate = Mapper.Map<ActivityTemplateDO, ActivityTemplateDTO>(activityTemplate);
                pluginId = activityTemplate.PluginID;
            }
            else
            {
                pluginId = actionDTO.ActivityTemplate.PluginID;
            }
           
            var plugin = ObjectFactory.GetInstance<IPlugin>().GetAll().FirstOrDefault(x => x.Id == pluginId);

            if (plugin == null || string.IsNullOrEmpty(plugin.Endpoint))
            {
                BaseUri = null;
            }
            else
        {
                BaseUri = new Uri(plugin.Endpoint.StartsWith("http") ? plugin.Endpoint : "http://" + plugin.Endpoint);
            }

            var actionName = Regex.Replace(curActionType, @"[^-_\w\d]", "_");
            var requestUri = new Uri(string.Format("actions/{0}", actionName), UriKind.Relative);

            return await PostAsync<ActionDTO, TResponse>(requestUri, actionDTO);
        }
    }
}
