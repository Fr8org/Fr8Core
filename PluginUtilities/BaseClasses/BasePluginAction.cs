using System.IO;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using PluginBase.Infrastructure;

namespace PluginBase.BaseClasses
{
    //this method allows a specific Action to inject its own evaluation function into the 
    //standard ProcessConfigurationRequest
    public delegate ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO);

    public class BasePluginAction
    {
        protected ConfigurationSettingsDTO ProcessConfigurationRequest(ActionDO curActionDO, ConfigurationEvaluator configurationEvaluationResult)
        {
            if (configurationEvaluationResult(curActionDO) == ConfigurationRequestType.Initial)
            {
                return InitialConfigurationResponse(curActionDO);
            }
            else if(configurationEvaluationResult(curActionDO) == ConfigurationRequestType.Followup)
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