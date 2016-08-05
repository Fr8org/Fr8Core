using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fr8.Infrastructure.Communication;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Utilities.Logging;
using StructureMap;
using Hub.Interfaces;

namespace Hub.Managers.APIManagers.Transmitters.Terminal
{
    public class TerminalTransmitter : RestfulServiceClient, ITerminalTransmitter
    {
        private readonly ITerminal _terminalService;

        public TerminalTransmitter(ITerminal terminalService)
        {
            _terminalService = terminalService;
        }

        /// <summary>
        /// Posts ActionDTO to "/activities/&lt;actionType&gt;"
        /// </summary>
        /// <param name="curActionType">Action Type</param>
        /// <param name="activityDTO">DTO</param>
        /// <remarks>Uses <paramref name="curActionType"/> argument for constructing request uri replacing all space characters with "_"</remarks>
        /// <returns></returns>
        public async Task<TResponse> CallActivityAsync<TResponse>(
            string curActionType,
            IEnumerable<KeyValuePair<string, string>> parameters,
            Fr8DataDTO dataDTO,
            string correlationId)
        {
            if (dataDTO == null)
            {
                throw new ArgumentNullException(nameof(dataDTO));
            }
            
            if (dataDTO.ActivityDTO == null)
            {
                throw new ArgumentNullException(nameof(dataDTO.ActivityDTO));
            }

            if (dataDTO.ActivityDTO.ActivityTemplate == null)
            {
                throw new ArgumentOutOfRangeException(nameof(dataDTO.ActivityDTO), "ActivityTemplate must be specified explicitly");
            }

            var activityTemplate = dataDTO.ActivityDTO.ActivityTemplate;
            var terminal = ObjectFactory.GetInstance<ITerminal>().GetByNameAndVersion(activityTemplate.TerminalName, activityTemplate.TerminalVersion);

            var actionName = Regex.Replace(curActionType, @"[^-_\w\d]", "_").ToLower();
            string queryString = string.Empty;
            if (parameters != null && parameters.Any())
            {
                var queryStringBuilder = new StringBuilder();
                queryStringBuilder.Append("?");
                foreach (var parameter in parameters)
                {
                    if (queryStringBuilder.Length > 1)
                    {
                        queryStringBuilder.Append("&");
                    }

                    queryStringBuilder.Append(WebUtility.UrlEncode(parameter.Key));
                    queryStringBuilder.Append("=");
                    queryStringBuilder.Append(WebUtility.UrlEncode(parameter.Value));
                }

                queryString = queryStringBuilder.ToString();
            }

            var requestUri = new Uri($"activities/{actionName}{queryString}", UriKind.Relative);
            if (string.IsNullOrEmpty(terminal?.Endpoint))
            {
                //_logger.ErrorFormat("Terminal record not found for activityTemplate: {0}. Throwing exception.", dataDTO.ActivityDTO.ActivityTemplate.Name);
                Logger.GetLogger().Error($"Terminal record not found for activityTemplate: {dataDTO.ActivityDTO.ActivityTemplate.Name} Throwing exception.");
                throw new Exception("Unknown terminal or terminal endpoint");
            }

            requestUri = new Uri(new Uri(terminal.Endpoint), requestUri);
            return await PostAsync<Fr8DataDTO, TResponse>(requestUri, dataDTO, correlationId, _terminalService.GetRequestHeaders(terminal, dataDTO.ActivityDTO.AuthToken.UserId));
        }
    }
}