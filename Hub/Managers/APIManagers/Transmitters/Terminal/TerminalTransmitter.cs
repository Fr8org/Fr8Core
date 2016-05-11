using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fr8Data.DataTransferObjects;
using StructureMap;
using Hub.Interfaces;
using Hub.Managers.APIManagers.Transmitters.Restful;
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
        /// Posts ActionDTO to "/activities/&lt;actionType&gt;"
        /// </summary>
        /// <param name="curActionType">Action Type</param>
        /// <param name="activityDTO">DTO</param>
        /// <remarks>Uses <paramref name="curActionType"/> argument for constructing request uri replacing all space characters with "_"</remarks>
        /// <returns></returns>
        public async Task<TResponse> CallActivityAsync<TResponse>(string curActionType, Fr8DataDTO dataDTO, string correlationId)
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

            var terminalDTO = dataDTO.ActivityDTO.ActivityTemplate.Terminal;
            var terminal = ObjectFactory.GetInstance<ITerminal>().GetByNameAndVersion(terminalDTO.Name, terminalDTO.Version);


            var actionName = Regex.Replace(curActionType, @"[^-_\w\d]", "_");
            var requestUri = new Uri(string.Format("activities/{0}", actionName), UriKind.Relative);
            if (terminal == null || string.IsNullOrEmpty(terminal.Endpoint))
            {
                //_logger.ErrorFormat("Terminal record not found for activityTemplate: {0}. Throwing exception.", dataDTO.ActivityDTO.ActivityTemplate.Name);
                Logger.LogError($"Terminal record not found for activityTemplate: {dataDTO.ActivityDTO.ActivityTemplate.Name} Throwing exception.");
                throw new Exception("Unknown terminal or terminal endpoint");
            }
            //let's calculate absolute url, since our hmac mechanism needs it
            requestUri = new Uri(new Uri(terminal.Endpoint), requestUri);
            var hmacHeader =  await _hmacService.GenerateHMACHeader(requestUri, terminal.PublicIdentifier, terminal.Secret, dataDTO.ActivityDTO.AuthToken.UserId, dataDTO);
            return await PostAsync<Fr8DataDTO, TResponse>(requestUri, dataDTO, correlationId, hmacHeader);
        }
    }
}