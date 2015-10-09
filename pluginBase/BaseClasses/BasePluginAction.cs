using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using PluginBase.Infrastructure;
using StructureMap;
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

        protected IAction _action;
        protected ICrate _crate;

        public BasePluginAction()
        {
            _crate = ObjectFactory.GetInstance<ICrate>();
            _action = ObjectFactory.GetInstance<IAction>();
        }

        protected bool IsEmptyAuthToken(ActionDTO actionDTO)
        {
            if (actionDTO == null
                || actionDTO.AuthToken == null
                || string.IsNullOrEmpty(actionDTO.AuthToken.Token))
            {
                return true;
            }

            return false;
        }

        protected void RemoveAuthenticationCrate(ActionDTO actionDTO)
        {
            if (actionDTO.CrateStorage != null
                && actionDTO.CrateStorage.CrateDTO != null)
            {
                var authCrates = actionDTO.CrateStorage.CrateDTO
                    .Where(x => x.ManifestType == CrateManifests.STANDARD_AUTHENTICATION_NAME)
                    .ToList();

                foreach (var authCrate in authCrates)
                {
                    actionDTO.CrateStorage.CrateDTO.Remove(authCrate);
                }
            }
        }

        protected void AppendDockyardAuthenticationCrate(
            ActionDTO actionDTO, AuthenticationMode mode)
        {
            if (actionDTO.CrateStorage == null)
            {
                actionDTO.CrateStorage = new CrateStorageDTO()
                {
                    CrateDTO = new List<CrateDTO>()
                };
            }

            actionDTO.CrateStorage.CrateDTO.Add(
                _crate.CreateAuthenticationCrate("RequiresAuthentication", mode)
            );
        }

        protected async Task<PayloadDTO> GetProcessPayload(int processId)
        {
            var httpClient = new HttpClient();
            var url = ConfigurationManager.AppSettings["CoreWebServerUrl"]
                + "api/processes/"
                + processId.ToString();

            using (var response = await httpClient.GetAsync(url))
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<PayloadDTO>(content);
            }
        }

        protected async Task<ActionDTO> ProcessConfigurationRequest(ActionDTO curActionDTO, ConfigurationEvaluator configurationEvaluationResult)
        {
            if (configurationEvaluationResult(curActionDTO) == ConfigurationRequestType.Initial)
            {
                return await InitialConfigurationResponse(curActionDTO);
            }

            else if (configurationEvaluationResult(curActionDTO) == ConfigurationRequestType.Followup)
            {
                return await FollowupConfigurationResponse(curActionDTO);
            }

            throw new InvalidDataException("Action's Configuration Store does not contain connection_string field.");
        }

        //if the Action doesn't provide a specific method to override this, we just return the existing CrateStorage, unchanged
        protected virtual async Task<ActionDTO> InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            //Returns Task<ActivityDTO> using FromResult as the return type is known
            return await Task.FromResult<ActionDTO>(curActionDTO);
        }

        //if the Action doesn't provide a specific method to override this, we just return the existing CrateStorage, unchanged
        protected virtual async Task<ActionDTO> FollowupConfigurationResponse(ActionDTO curActionDTO)
        {
            //Returns Task<ActivityDTO> using FromResult as the return type is known
            return await Task.FromResult<ActionDTO>(curActionDTO);
        }

        protected async virtual Task<List<CrateDTO>> GetCratesByDirection(int activityId,
            string manifestType, GetCrateDirection direction)
        {
            var httpClient = new HttpClient();

            // TODO: after DO-1214 this must target to "ustream" and "downstream" accordingly.
            var directionSuffix = (direction == GetCrateDirection.Upstream)
                ? "upstream_actions/"
                : "downstream_actions/";

            var url = ConfigurationManager.AppSettings["CoreWebServerUrl"]
                + "activities/"
                + directionSuffix
                + "?id=" + activityId.ToString();

            using (var response = await httpClient.GetAsync(url))
            {
                var content = await response.Content.ReadAsStringAsync();
                var curActions = JsonConvert.DeserializeObject<List<ActionDTO>>(content);

                var curCrates = new List<CrateDTO>();

                foreach (var curAction in curActions)
                {
                    curCrates.AddRange(_action.GetCratesByManifestType(manifestType, curAction.CrateStorage).ToList());
                }

                return curCrates;
            }
        }

        public async Task<StandardDesignTimeFieldsMS> GetDesignTimeFields(
            int activityId, GetCrateDirection direction)
        {

            //1) Build a merged list of the upstream design fields to go into our drop down list boxes
            StandardDesignTimeFieldsMS mergedFields = new StandardDesignTimeFieldsMS();

            List<CrateDTO> curCrates = await GetCratesByDirection(
                activityId,
                CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME,
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

        protected CrateStorageDTO AssembleCrateStorage(params CrateDTO[] curCrates)
        {
            return new CrateStorageDTO()
            {
                CrateDTO = curCrates.ToList()
            };
        }

        protected CrateDTO PackControlsCrate(params ControlsDefinitionDTO[] controlsList)
        {
            var controlsCrate = _crate.CreateStandardConfigurationControlsCrate(
                "Configuration_Controls", controlsList);

            return controlsCrate;
        }

        protected string ExtractControlFieldValue(ActionDTO curActionDto, string fieldName)
        {
            var controlsCrate = curActionDto.CrateStorage.CrateDTO
                .FirstOrDefault(
                    x => x.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME
                    && x.Label == "Configuration_Controls");

            if (controlsCrate == null)
            {
                throw new ApplicationException("No Configuration_Controls crate found.");
            }

            var controlsCrateMS = JsonConvert
                .DeserializeObject<StandardConfigurationControlsMS>(
                    controlsCrate.Contents
                );

            var field = controlsCrateMS.Controls
                .FirstOrDefault(x => x.Name == fieldName);

            if (field == null)
            {
                return null;
            }

            return field.Value;
        }
    }
}