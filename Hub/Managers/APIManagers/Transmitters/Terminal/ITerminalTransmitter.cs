using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;

namespace Hub.Managers.APIManagers.Transmitters.Terminal
{
    public interface ITerminalTransmitter : IRestfulServiceClient
    {
        /// <summary>
        /// Posts a DTO to terminal API
        /// </summary>
        /// <param name="actionType">Action type</param>
        /// <param name="activityDTO">ActionDTO</param>
        /// <param name="correlationId"></param>
        /// <param name="userId"></param>
        /// <param name="terminalId"></param>
        /// <param name="terminalSecret"></param>
        /// <returns></returns>
        Task<TResponse> CallActivityAsync<TResponse>(
            string actionType,
            IEnumerable<KeyValuePair<string, string>> parameters,
            Fr8DataDTO dataDTO,
            string correlationId
        );
    }
}