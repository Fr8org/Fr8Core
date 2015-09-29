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
        /// <param name="actionDTO">DTO</param>
        /// <returns></returns>
        Task<ActionDTO> CallActionAsync<T>(string actionType, T dto);

        PluginDO Plugin { get; set; }
    }
}