using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Data.Constants;
using StructureMap;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Hub.Services;

namespace Hub.Managers.APIManagers.Transmitters.Terminal
{
    public class TerminalTransmitter : RestfulServiceClient, ITerminalTransmitter
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
            return await CallActionAsync<TResponse>(curActionType, null, actionDTO);
        }

        public async Task<PayloadDTO> RunActionAsync(ActionState actionState, ActionDTO actionDTO)
        {
            return await CallActionAsync<PayloadDTO>("Run", actionState, actionDTO);
        }

        private async Task<TResponse> CallActionAsync<TResponse>(string actionType, ActionState? actionState, ActionDTO actionDTO)
        {
            if (actionDTO == null)
            {
                throw new ArgumentNullException("actionDTO");
            }

            if ((actionDTO.ActivityTemplateId == null || actionDTO.ActivityTemplateId == 0) && actionDTO.ActivityTemplate == null)
            {
                throw new ArgumentOutOfRangeException("actionDTO", actionDTO.ActivityTemplateId, "ActivityTemplate must be specified either explicitly or by using ActivityTemplateId");
            }

            int terminalId;

            if (actionDTO.ActivityTemplate == null)
            {
                var activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>().GetByKey(actionDTO.ActivityTemplateId.Value);
                actionDTO.ActivityTemplate = Mapper.Map<ActivityTemplateDO, ActivityTemplateDTO>(activityTemplate);
                terminalId = activityTemplate.TerminalId;
            }
            else
            {
                terminalId = actionDTO.ActivityTemplate.TerminalId;
            }

            var terminal = ObjectFactory.GetInstance<ITerminal>().GetAll().FirstOrDefault(x => x.Id == terminalId);


            if (terminal == null || string.IsNullOrEmpty(terminal.Endpoint))
            {
                BaseUri = null;
            }
            else
            {
                BaseUri = new Uri(terminal.Endpoint.StartsWith("http") ? terminal.Endpoint : "http://" + terminal.Endpoint);
            }

            var actionName = Regex.Replace(actionType, @"[^-_\w\d]", "_");
            Uri requestUri;
            if (actionState == null)
            {
                requestUri = new Uri(string.Format("actions/Execute?type={0}", actionName), UriKind.Relative);
            }
            else
            {
                
                requestUri = new Uri(string.Format("actions/Execute?type=Run&state={0}", actionState), UriKind.Relative);
            }


            return await PostAsync<ActionDTO, TResponse>(requestUri, actionDTO);
        }
    }
}
