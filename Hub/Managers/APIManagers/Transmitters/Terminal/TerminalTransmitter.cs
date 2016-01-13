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
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public TerminalTransmitter()
        {
            _hmacService = ObjectFactory.GetInstance<IHMACService>();
        }

        public static long GetCurrentUnixTimestampSeconds()
        {
            return (long)(DateTime.UtcNow - UnixEpoch).TotalSeconds;
        }

        private async Task<Dictionary<string, string>> GetHMACHeader(Uri requestUri, string userId, string terminalId, string terminalSecret, ActionDTO content)
        {
            var timeStamp = GetCurrentUnixTimestampSeconds().ToString(CultureInfo.InvariantCulture);
            var nonce = Guid.NewGuid().ToString();
            var hash = await _hmacService.CalculateHMACHash(requestUri, userId, terminalId, terminalSecret, timeStamp, nonce, content);
            var mergedData = string.Format("{0}:{1}:{2}:{3}:{4}", terminalId, hash, nonce, timeStamp, userId);
            return new Dictionary<string, string>()
            {
                {System.Net.HttpRequestHeader.Authorization.ToString(), string.Format("hmac {0}", mergedData)}
            };   
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

            var actionName = Regex.Replace(curActionType, @"[^-_\w\d]", "_");
            var requestUri = new Uri(string.Format("actions/{0}", actionName), UriKind.Relative);

            Dictionary<string, string> hmacHeader;
            if (terminal != null) { 
                hmacHeader = await GetHMACHeader(requestUri, actionDTO.AuthToken.UserId, terminal.Id.ToString(CultureInfo.InvariantCulture), terminal.Secret, actionDTO);
            }

            return await PostAsync<ActionDTO, TResponse>(requestUri, actionDTO, correlationId, hmacHeader);
        }
    }
}