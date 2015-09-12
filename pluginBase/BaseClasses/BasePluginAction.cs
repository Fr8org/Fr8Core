using System.IO;
using System.Linq;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using PluginBase.Infrastructure;
using StructureMap;
using System;

namespace PluginBase.BaseClasses
{
    //this method allows a specific Action to inject its own evaluation function into the 
    //standard ProcessConfigurationRequest
    public delegate ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO);

    public class BasePluginAction
    {
        public enum GetCrateDirection
        {
            Upstream,
            Downstream
        }

        protected const int STANDARD_PAYLOAD_MANIFEST_ID = 5;
        protected const string STANDARD_PAYLOAD_MANIFEST_NAME = "Standard Payload Data";

        protected CrateStorageDTO ProcessConfigurationRequest(ActionDO curActionDO, ConfigurationEvaluator configurationEvaluationResult)
        {
            if (configurationEvaluationResult(curActionDO) == ConfigurationRequestType.Initial)
            {
                return InitialConfigurationResponse(curActionDO);
            }

            else if (configurationEvaluationResult(curActionDO) == ConfigurationRequestType.Followup)
            {
                return FollowupConfigurationResponse(curActionDO);
            }

            throw new InvalidDataException("Action's Configuration Store does not contain connection_string field.");
        }

        //if the Action doesn't provide a specific method to override this, we just return the existing ConfigurationStore, unchanged
        protected virtual CrateStorageDTO InitialConfigurationResponse(ActionDO curActionDO)
        {
            return curActionDO.CrateStorageDTO();
        }

        //if the Action doesn't provide a specific method to override this, we just return the existing ConfigurationStore, unchanged
        protected virtual CrateStorageDTO FollowupConfigurationResponse(ActionDO curActionDO)
        {
            return curActionDO.CrateStorageDTO();
        }

        protected virtual CrateDTO GetCrate(ActivityDO activityDO,
            string searchId, GetCrateDirection direction)
        {
            return GetCrate(activityDO, x => x.Id == searchId, direction);
        }

        protected virtual CrateDTO GetCrate(ActivityDO activityDO, Func<CrateDTO, bool>predicate, GetCrateDirection direction)
        {
            var curActivityService = ObjectFactory.GetInstance<IActivity>();

            var curUpstreamActivities = (direction == GetCrateDirection.Upstream)
                ? curActivityService.GetUpstreamActivities(activityDO)
                : curActivityService.GetDownstreamActivities(activityDO);

            foreach (var curUpstreamAction in curUpstreamActivities.OfType<ActionDO>())
            {
                var curCrateStorage = curUpstreamAction.CrateStorageDTO();
                var curCrate = curCrateStorage.CratesDTO.FirstOrDefault(predicate);

                if (curCrate != null)
                {
                    return curCrate;
                }
            }

            return null;
        }
    }
}