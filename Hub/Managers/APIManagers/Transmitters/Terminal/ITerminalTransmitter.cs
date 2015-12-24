using System.Threading.Tasks;
using Data.Constants;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Hub.Managers.APIManagers.Transmitters.Restful;

namespace Hub.Managers.APIManagers.Transmitters.Terminal
{
    public interface ITerminalTransmitter : IRestfulServiceClient
    {
        /// <summary>
        /// Posts a DTO to terminal API
        /// </summary>
        /// <param name="actionType">Action type</param>
        /// <param name="actionDTO">ActionDTO</param>
        /// <returns></returns>
        Task<TResponse> CallActionAsync<TResponse>(string actionType, ActionDTO actionDTO);
    }
}