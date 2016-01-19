using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using AutoMapper;
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
        private readonly IHMACService _hmacService;
        private readonly IActivityTemplate _activityTemplate;

        public TerminalTransmitter()
        {
            _hmacService = ObjectFactory.GetInstance<IHMACService>();
            _activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>();
        }

        /// <summary>
        /// Posts ActionDTO to "/actions/&lt;actionType&gt;"
        /// </summary>
        /// <param name="curActionType">Action Type</param>
        /// <param name="actionDTO">DTO</param>
        /// <remarks>Uses <paramref name="curActionType"/> argument for constructing request uri replacing all space characters with "_"</remarks>
        /// <returns></returns>
        public async Task<TResponse> CallActionAsync<TResponse>(string curActionType, ActionDTO actionDTO, string correlationId)
        {
            if (actionDTO == null)
            {
                throw new ArgumentNullException("actionDTO");
            }

            if (actionDTO.ActivityTemplateId == null )
            {
                throw new ArgumentOutOfRangeException("actionDTO", actionDTO.ActivityTemplateId, "ActivityTemplate must be specified by using ActivityTemplateId");
            }

            var terminal = _activityTemplate.GetByKey(actionDTO.ActivityTemplateId.Value).Terminal;
           
            var actionName = Regex.Replace(curActionType, @"[^-_\w\d]", "_");
            var requestUri = new Uri(string.Format("actions/{0}", actionName), UriKind.Relative);
            if (terminal == null || string.IsNullOrEmpty(terminal.Endpoint))
            {
                throw new Exception("Unknown terminal or terminal endpoint");
            }
            //let's calculate absolute url, since our hmac mechanism needs it
            requestUri = new Uri(new Uri(terminal.Endpoint.StartsWith("http") ? terminal.Endpoint : "http://" + terminal.Endpoint), requestUri);
            var hmacHeader = await _hmacService.GenerateHMACHeader(requestUri, terminal.PublicIdentifier, terminal.Secret, actionDTO.AuthToken.UserId, actionDTO);
            return await PostAsync<ActionDTO, TResponse>(requestUri, actionDTO, correlationId, hmacHeader);
        }
    }
}