using System;
using System.Configuration;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using PluginBase.BaseClasses;
using PluginBase.Infrastructure;

namespace pluginAzureSqlServer.Actions
{
    public class Monitor_Channel_v1 : BasePluginAction
    {
        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            if (IsEmptyAuthToken(curActionDTO))
            {
                AppendDockyardAuthenticationCrate(
                    curActionDTO,
                    AuthenticationMode.ExternalMode,
                    CreateAuthUrl());
                return curActionDTO;
            }

            RemoveAuthenticationCrate(curActionDTO);

            return await ProcessConfigurationRequest(curActionDTO,
                x => ConfigurationEvaluator(x));
        }

        private ConfigurationRequestType ConfigurationEvaluator(ActionDTO curActionDTO)
        {
            var crateStorage = curActionDTO.CrateStorage;

            if (crateStorage.CrateDTO.Count == 0)
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        /// <summary>
        /// Build external Slack OAuth url.
        /// </summary>
        private string CreateAuthUrl()
        {
            var template = ConfigurationManager.AppSettings["SlackOAuthUrl"];
            var url = template.Replace("%STATE%", Guid.NewGuid().ToString());

            return url;
        }
    }
}