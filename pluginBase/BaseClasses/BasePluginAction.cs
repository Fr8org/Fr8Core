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
using Data.Interfaces.ManifestSchemas;
using Data.States.Templates;
using Newtonsoft.Json;

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

        protected IAction _action;
        protected ICrate _crate;
        protected IActivity _activity;

        public BasePluginAction()
        {
            _crate = ObjectFactory.GetInstance<ICrate>();
            _action = ObjectFactory.GetInstance<IAction>();
            //_activity = ObjectFactory.GetInstance<IActivity>();
        }
        protected ActionDTO ProcessConfigurationRequest(ActionDTO curActionDTO, ConfigurationEvaluator configurationEvaluationResult)
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
        protected virtual ActionDTO InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            return curActionDTO;
        }

        //if the Action doesn't provide a specific method to override this, we just return the existing CrateStorage, unchanged
        protected virtual ActionDTO FollowupConfigurationResponse(ActionDTO curActionDTO)
        {
            return curActionDTO;
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

        public StandardDesignTimeFieldsMS GetDesignTimeFields(ActionDO curActionDO, GetCrateDirection direction)
        {

            //1) Build a merged list of the upstream design fields to go into our drop down list boxes
            StandardDesignTimeFieldsMS mergedFields = new StandardDesignTimeFieldsMS();

            List<CrateDTO> curCrates = GetCratesByDirection(curActionDO, "Standard Design-Time Fields",
                direction);

            mergedFields.Fields.AddRange(MergeContentFields(curCrates).Fields);

            return mergedFields;
        }


        public StandardDesignTimeFieldsMS MergeContentFields(List<CrateDTO> curCrates)
        {
            StandardDesignTimeFieldsMS tempMS = new StandardDesignTimeFieldsMS();
            foreach (var curCrate in curCrates)
            {
                //extract the fields
                StandardDesignTimeFieldsMS curStandardDesignTimeFieldsCrate =
                    JsonConvert.DeserializeObject<StandardDesignTimeFieldsMS>(curCrate.Contents);

                //add them to the pile
                tempMS.Fields.AddRange(curStandardDesignTimeFieldsCrate.Fields);
            }

            return tempMS;
        }

        protected CrateStorageDTO AssembleCrateStorage(List<CrateDTO> curCrates)
        {
            return new CrateStorageDTO()
            {
                CrateDTO = curCrates
            };
        }

        protected CrateDTO PackControlsCrate(List<FieldDefinitionDTO> controlsList)
        {
            var controlsMS = new StandardConfigurationControlsMS()
            {
                Controls = controlsList
            };

            var controlsCrate = _crate.CreateStandardConfigurationControlsCrate(
                "Configuration_Controls", controlsMS);


            return controlsCrate;
        }


    }
}