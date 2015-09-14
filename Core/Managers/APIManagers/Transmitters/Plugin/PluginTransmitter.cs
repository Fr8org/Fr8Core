using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Core.Managers.APIManagers.Transmitters.Restful;
using Data.Interfaces.DataTransferObjects;

namespace Core.Managers.APIManagers.Transmitters.Plugin
{
    public class PluginTransmitter : RestfulServiceClient, IPluginTransmitter
    {
        /// <summary>
        /// Posts ActionDTO object to "/actions/&lt;actionType&gt;"
        /// </summary>
        /// <param name="curActionType">Action Type</param>
        /// <param name="curActionDTO"></param>
        /// <remarks>Uses <paramref name="curActionType"/> argument for constructing request uri replacing all space characters with "_"</remarks>
        /// <returns></returns>
        public async Task<string> PostActionAsync(string curActionType, ActionDTO curActionDTO, PayloadDTO curPayloadDTO)
        {
            var action = Regex.Replace(curActionType, @"[^-_\w\d]", "_");
            var requestUri = new Uri(string.Format("actions/{0}", action), UriKind.Relative);
            var dataPackage = new ActionDataPackageDTO(curActionDTO, curPayloadDTO);

            return await PostAsync(requestUri, dataPackage);
        }
    }
}
