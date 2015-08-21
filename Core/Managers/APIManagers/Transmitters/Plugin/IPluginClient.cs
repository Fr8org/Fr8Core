using System.Threading.Tasks;
using Core.Managers.APIManagers.Transmitters.Restful;
using Data.Interfaces.DataTransferObjects;

namespace Core.Managers.APIManagers.Transmitters.Plugin
{
    public interface IPluginClient : IRestfulServiceClient
    {
        /// <summary>
        /// Posts ActionDTO object to plugin API
        /// </summary>
        /// <param name="actionType">Action type</param>
        /// <param name="actionDTO">Action DTO</param>
        /// <returns></returns>
        Task<string> PostActionAsync(string actionType, ActionDTO actionDTO);
    }
}