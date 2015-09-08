using System.IO;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using PluginBase.Infrastructure;

namespace PluginBase.BaseClasses
{
    public delegate ConfigurationRequestType DetermineConfigurationRequestType(ActionDO curActionDO);

    public class BasePluginAction
    {
        protected ConfigurationSettingsDTO ProcessConfigurationRequest(ActionDO curActionDO, DetermineConfigurationRequestType curConfigurationRequestChecker)
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

        //if the Action doesn't provide a specific method to override this, we just return the existing ConfigurationStore, unchanged
        protected virtual ConfigurationSettingsDTO InitialConfigurationResponse(ActionDO curActionDO)
        {
            return curActionDO.ConfigurationSettingsDTO();
        }
        //if the Action doesn't provide a specific method to override this, we just return the existing ConfigurationStore, unchanged
        protected virtual ConfigurationSettingsDTO FollowupConfigurationResponse(ActionDO curActionDO)
        {
            return curActionDO.ConfigurationSettingsDTO();
        }
    }
}