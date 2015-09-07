
using System;
using System.IO;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;

namespace PluginUtilities.Infrastructure
{

    public delegate ConfigurationRequestType DetermineConfigurationRequestType(ActionDO curActionDO);

    public abstract class BasePluginAction
    {
        protected ConfigurationSettingsDTO DetermineConfigurationRequest(ActionDO curActionDO, DetermineConfigurationRequestType curConfigurationRequestChecker)
        {
            if (curConfigurationRequestChecker(curActionDO) == ConfigurationRequestType.Initial)
            {
                return InitialConfigurationResponse(curActionDO);
            }
            else if(curConfigurationRequestChecker(curActionDO) == ConfigurationRequestType.Followup)
            {
                return FollowupConfigurationResponse(curActionDO);
            }

            throw new InvalidDataException("Action's Configuration Store does not contain connection_string field.");
        }

        protected abstract ConfigurationSettingsDTO InitialConfigurationResponse(ActionDO curActionDO);

        protected abstract ConfigurationSettingsDTO FollowupConfigurationResponse(ActionDO curActionDO);
    }
}