using System.IO;
using System.Linq;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using PluginBase.Infrastructure;
using StructureMap;
using System;
using System.Collections.Generic;
using AutoMapper;
using Data.States.Templates;

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

        private IAction _action;

        public BasePluginAction()
        {
            _action = ObjectFactory.GetInstance<IAction>();
        }
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

        //protected virtual CrateDTO GetCratesByDirection(ActionDTO actionDTO,
        //    string manifestType, GetCrateDirection direction)
        //{
        //    return GetCratesByDirection(actionDTO, x => x.ManifestType == manifestType, direction);
        //}

        //protected virtual CrateDTO GetCratesByDirection(ActionDTO actionDTO, Func<CrateDTO, bool>predicate, GetCrateDirection direction)
        //{
        //    var actionDO = Mapper.Map<ActionDO>(actionDTO);
        //    return GetCratesByDirection(actionDO, predicate, direction);
        //}

        protected virtual List<CrateDTO> GetCratesByDirection(ActivityDO activityDO, string manifestType, GetCrateDirection direction)
        {
            var curActivityService = ObjectFactory.GetInstance<IActivity>();

            var curUpstreamActivities = (direction == GetCrateDirection.Upstream)
                ? curActivityService.GetUpstreamActivities(activityDO)
                : curActivityService.GetDownstreamActivities(activityDO);

            List<CrateDTO> upstreamCrates = new List<CrateDTO>();

            //assemble all of the crates belonging to upstream activities
            foreach (var curAction in curUpstreamActivities.OfType<ActionDO>())
            {
                upstreamCrates.AddRange(_action.GetCratesByManifestType(manifestType, curAction.CrateStorageDTO()).ToList());            
            }

            return upstreamCrates;
        }


    }
}