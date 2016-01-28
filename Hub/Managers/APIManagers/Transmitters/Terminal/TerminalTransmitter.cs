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
using Utilities.Logging;

namespace Hub.Managers.APIManagers.Transmitters.Terminal
{
    public class TerminalTransmitter : RestfulServiceClient, ITerminalTransmitter
    {
        private readonly IHMACService _hmacService;
        log4net.ILog _logger;

        public TerminalTransmitter()
        {
            _hmacService = ObjectFactory.GetInstance<IHMACService>();
            _logger = Logger.GetLogger();
        }

        /// <summary>
        /// Posts ActionDTO to "/actions/&lt;actionType&gt;"
        /// </summary>
        /// <param name="curActionType">Action Type</param>
        /// <param name="activityDTO">DTO</param>
        /// <remarks>Uses <paramref name="curActionType"/> argument for constructing request uri replacing all space characters with "_"</remarks>
        /// <returns></returns>
        public async Task<TResponse> CallActionAsync<TResponse>(string curActionType, ActivityDTO activityDTO, string correlationId)
        {
            if (activityDTO == null)
            {
                throw new ArgumentNullException("activityDTO");
            }

            if ((activityDTO.ActivityTemplateId == null || activityDTO.ActivityTemplateId == 0) && activityDTO.ActivityTemplate == null)
            {
                throw new ArgumentOutOfRangeException("activityDTO", activityDTO.ActivityTemplateId, "ActivityTemplate must be specified either explicitly or by using ActivityTemplateId");
            }

            int terminalId;

            if (activityDTO.ActivityTemplate == null)
            {
                var activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>().GetByKey(activityDTO.ActivityTemplateId.Value);
                activityDTO.ActivityTemplate = Mapper.Map<ActivityTemplateDO, ActivityTemplateDTO>(activityTemplate);
                terminalId = activityTemplate.TerminalId;
                _logger.DebugFormat("ActivityTemplate found: {0}", activityTemplate != null);
                _logger.DebugFormat("Terminal id: {0}", terminalId);
            }
            else
            {
                terminalId = activityDTO.ActivityTemplate.TerminalId;
            }

            var terminal = ObjectFactory.GetInstance<ITerminal>().GetAll().FirstOrDefault(x => x.Id == terminalId);

            
            var actionName = Regex.Replace(curActionType, @"[^-_\w\d]", "_");
            var requestUri = new Uri(string.Format("actions/{0}", actionName), UriKind.Relative);
            if (terminal == null || string.IsNullOrEmpty(terminal.Endpoint))
            {
                _logger.ErrorFormat("Terminal record not found for activityTemplateId: {0}. Throwing exception.", activityDTO.ActivityTemplateId);
                throw new Exception("Unknown terminal or terminal endpoint");
            }
            //let's calculate absolute url, since our hmac mechanism needs it
            requestUri = new Uri(new Uri(terminal.Endpoint.StartsWith("http") ? terminal.Endpoint : "http://" + terminal.Endpoint), requestUri);
            var hmacHeader = await _hmacService.GenerateHMACHeader(requestUri, terminal.PublicIdentifier, terminal.Secret, activityDTO.AuthToken.UserId, activityDTO);
            return await PostAsync<ActivityDTO, TResponse>(requestUri, activityDTO, correlationId, hmacHeader);
        }
    }
}