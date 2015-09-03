using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Core.Managers.APIManagers.Transmitters.Restful;
using Data.Interfaces.DataTransferObjects;

namespace Core.Managers.APIManagers.Transmitters.Plugin
{
    class PluginTransmitter : RestfulServiceClient, IPluginTransmitter
    {
        /// <summary>
        /// Posts ActionDTO object to "/actions/&lt;actionType&gt;"
        /// </summary>
        /// <param name="actionType">Action Type</param>
        /// <param name="actionDTO"></param>
        /// <remarks>Uses <paramref name="actionType"/> argument for constructing request uri replacing all space characters with "_"</remarks>
        /// <returns></returns>
        public async Task<string> PostActionAsync(string actionType, ActionPayloadDTO actionPayloadDTO)
        {
            var action = Regex.Replace(actionType, @"[^-_\w\d]", "_");
            var requestUri = new Uri(string.Format("actions/{0}", action), UriKind.Relative);
            return await PostAsync(requestUri, actionPayloadDTO);
        }
    }
}
