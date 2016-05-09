using System.Threading.Tasks;
using Fr8Data.DataTransferObjects;
using Infrastructure.Communication;

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
        Task<TResponse> CallActivityAsync<TResponse>(string actionType, Fr8DataDTO dataDTO, string correlationId);
    }
}