using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Core.Managers.APIManagers.Transmitters.Restful;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;

namespace Core.Managers.APIManagers.Transmitters.Plugin
{
    public class PluginTransmitter : RestfulServiceClient, IPluginTransmitter
    {
        private PluginDO _plugin;

        public PluginDO Plugin
        {
            get { return _plugin; }
            set
            {
                _plugin = value;
                if (_plugin != null && !string.IsNullOrEmpty(_plugin.Endpoint))
                {
                    BaseUri = new Uri(string.Concat(@"http://", _plugin.Endpoint), UriKind.Absolute);
                }
                else
                {
                    BaseUri = null;
                }
            }
        }

        /// <summary>
        /// Posts ActionDTO to "/actions/&lt;actionType&gt;"
        /// </summary>
        /// <param name="curActionType">Action Type</param>
        /// <param name="dto">DTO</param>
        /// <remarks>Uses <paramref name="curActionType"/> argument for constructing request uri replacing all space characters with "_"</remarks>
        /// <returns></returns>
        public async Task<R> CallActionAsync<T, R>(string curActionType, T dto)
        {
            var action = Regex.Replace(curActionType, @"[^-_\w\d]", "_");
            var requestUri = new Uri(string.Format("actions/{0}", action), UriKind.Relative);

            return await PostAsync<T, R>(requestUri, dto);
        }
    }
}
