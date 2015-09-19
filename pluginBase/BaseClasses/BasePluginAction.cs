using System.IO;
using System.Linq;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using PluginBase.Infrastructure;
using StructureMap;
using System;
using AutoMapper;

namespace PluginBase.BaseClasses
{
    //this method allows a specific Action to inject its own evaluation function into the 
    //standard ProcessConfigurationRequest
    public delegate ConfigurationRequestType ConfigurationEvaluator(ActionDTO curActionDTO);

    public class BasePluginAction
    {
        public enum GetCrateDirection
        {
            Upstream,
            Downstream
        }

        protected const int STANDARD_PAYLOAD_MANIFEST_ID = 5;
        protected const string STANDARD_PAYLOAD_MANIFEST_NAME = "Standard Payload Data";

        protected const int DESIGNTIME_FIELDS_MANIFEST_ID = 3;
        protected const string DESIGNTIME_FIELDS_MANIFEST_NAME = "Standard Design-Time Fields";

        //protected const int STANDARD_CONF_CONTROLS_MANIFEST_ID = ;
        protected const string STANDARD_CONF_CONTROLS_NANIFEST_NAME = "Standard Configuration Controls";

        protected CrateStorageDTO ProcessConfigurationRequest(ActionDTO curActionDTO, ConfigurationEvaluator configurationEvaluationResult)
        {
            if (configurationEvaluationResult(curActionDTO) == ConfigurationRequestType.Initial)
            {
                return InitialConfigurationResponse(curActionDTO);
            }

            else if (configurationEvaluationResult(curActionDTO) == ConfigurationRequestType.Followup)
            {
                return FollowupConfigurationResponse(curActionDTO);
            }

            throw new InvalidDataException("Action's Configuration Store does not contain connection_string field.");
        }

        //if the Action doesn't provide a specific method to override this, we just return the existing CrateStorage, unchanged
        protected virtual CrateStorageDTO InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            return curActionDTO.CrateStorage;
        }

        //if the Action doesn't provide a specific method to override this, we just return the existing CrateStorage, unchanged
        protected virtual CrateStorageDTO FollowupConfigurationResponse(ActionDTO curActionDTO)
        {
            return curActionDTO.CrateStorage;
        }

        protected virtual CrateDTO GetCrate(ActivityDO activityDO,
            string searchId, GetCrateDirection direction)
        {
            return GetCrate(activityDO, x => x.ManifestType == searchId, direction);
        }

        protected virtual CrateDTO GetCrate(ActionDTO actionDTO,
            string searchId, GetCrateDirection direction)
        {
            return GetCrate(actionDTO, x => x.ManifestType == searchId, direction);
        }

        protected virtual CrateDTO GetCrate(ActionDTO actionDTO, Func<CrateDTO, bool>predicate, GetCrateDirection direction)
        {
            var actionDO = Mapper.Map<ActionDO>(actionDTO);
            return GetCrate(actionDO, predicate, direction);
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
                var curCrate = curCrateStorage.CrateDTO.Last(predicate);
                //var curCrate = curCrateStorage.CrateDTO.FirstOrDefault(predicate);

                if (curCrate != null)
                {
                    return curCrate;
                }
            }

            return null;
        }


    }
}