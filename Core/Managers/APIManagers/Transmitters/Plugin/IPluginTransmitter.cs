using System.Threading.Tasks;
using Core.Managers.APIManagers.Transmitters.Restful;
using Data.Interfaces.DataTransferObjects;

namespace Core.Managers.APIManagers.Transmitters.Plugin
{
    public interface IPluginTransmitter : IRestfulServiceClient
    {
        /// <summary>
        /// Posts ActionPayloadDTO object to plugin API
        /// </summary>
        /// <param name="actionType">Action type</param>
        /// <param name="actionDTO">Action Payload DTO</param>
        /// <returns></returns>
        Task PostActionAsync(string actionType, ActionPayloadDTO actionPayloadDTO);
    }
}