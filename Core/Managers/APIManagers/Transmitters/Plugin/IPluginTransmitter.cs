using System.Threading.Tasks;
using Core.Managers.APIManagers.Transmitters.Restful;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;

namespace Core.Managers.APIManagers.Transmitters.Plugin
{
    public interface IPluginTransmitter : IRestfulServiceClient
    {
        /// <summary>
        /// Posts a DTO to plugin API
        /// </summary>
        /// <param name="actionType">Action type</param>
        /// <param name="actionDTO">ActionDTO</param>
        /// <returns></returns>
        Task<TResponse> CallActionAsync<TResponse>(string actionType, ActionDTO actionDTO);
    }
}