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
using fr8.Microsoft.Azure;

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
            var url = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
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

        /// <summary>
        /// Configure infrastructure.
        /// </summary>
        public virtual async Task<ActionDTO> Configure(ActionDTO actionDTO)
        {
            return await ProcessConfigurationRequest(actionDTO, ConfigurationEvaluator);
        }

        /// <summary>
        /// This method "evaluates" as to what configuration should be called. 
        /// Every plugin action will have its own decision making; hence this method must be implemented in the relevant child class.
        /// </summary>
        /// <param name="curActionDTO"></param>
        /// <returns></returns>
        public virtual ConfigurationRequestType ConfigurationEvaluator(ActionDTO curActionDTO)
        {
            throw new NotImplementedException("ConfigurationEvaluator method not implemented in child class.");
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

            var url = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
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

        public async Task<StandardDesignTimeFieldsCM> GetDesignTimeFields(
            int activityId, GetCrateDirection direction)
        {

            //1) Build a merged list of the upstream design fields to go into our drop down list boxes
            StandardDesignTimeFieldsCM mergedFields = new StandardDesignTimeFieldsCM();

            List<CrateDTO> curCrates = await GetCratesByDirection(
                activityId,
                CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME,
                direction);

            mergedFields.Fields.AddRange(MergeContentFields(curCrates).Fields);

            return mergedFields;
        }

        public StandardDesignTimeFieldsCM MergeContentFields(List<CrateDTO> curCrates)
        {
            StandardDesignTimeFieldsCM tempMS = new StandardDesignTimeFieldsCM();
            foreach (var curCrate in curCrates)
            {
                //extract the fields
                StandardDesignTimeFieldsCM curStandardDesignTimeFieldsCrate =
                    JsonConvert.DeserializeObject<StandardDesignTimeFieldsCM>(curCrate.Contents);

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

        protected CrateDTO PackControlsCrate(params ControlDefinitionDTO[] controlsList)
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
                .DeserializeObject<StandardConfigurationControlsCM>(
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

        protected async virtual Task<List<CrateDTO>> GetUpstreamFileHandleCrates(int curActionId)
        {
            return await GetCratesByDirection(curActionId, CrateManifests.STANDARD_FILE_HANDLE_MANIFEST_NAME, GetCrateDirection.Upstream);
        }

        protected async Task<CrateDTO> MergeUpstreamFields(int curActionDOId, string label)
        {
            var curUpstreamFields = (await GetDesignTimeFields(curActionDOId, GetCrateDirection.Upstream)).Fields.ToArray();
            CrateDTO upstreamFieldsCrate = _crate.CreateDesignTimeFieldsCrate(label, curUpstreamFields);

            return upstreamFieldsCrate;
        }

        protected ConfigurationRequestType ReturnInitialUnlessExistsField(ActionDTO curActionDTO, string fieldName, Manifest curSchema)
        {
            CrateStorageDTO curCrates = curActionDTO.CrateStorage;

            if (curCrates.CrateDTO.Count == 0)
                return ConfigurationRequestType.Initial;

            ActionDO curActionDO = Mapper.Map<ActionDO>(curActionDTO);

            //load configuration crates of manifest type Standard Control Crates
            //look for a text field name select_file with a value
            Manifest manifestSchema = new Manifest(Data.Constants.MT.StandardConfigurationControls);

            var keys = _action.FindKeysByCrateManifestType(curActionDO, manifestSchema, fieldName)
                .Select(e => (string)e["value"])
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();

            //if there are more than 2 return keys, something is wrong
            //if there are none or if there's one but it's value is "" the return initial else return followup
            Validations.ValidateMax1(keys);

            if (keys.Length == 0)
                return ConfigurationRequestType.Initial;
            else
            {
                return ConfigurationRequestType.Followup;
            }
        }

        //Returning the crate with text field control 
        protected CrateDTO GetTextBoxControlForDisplayingError(
            string fieldLabel, string errorMessage)
        {
            var fields = new List<ControlDefinitionDTO>() 
            {
                new TextBlockControlDefinitionDTO()
                {
                    Label = fieldLabel,
                    Value = errorMessage,
                    CssClass = "well well-lg"                    
                }
            };

            var controls = new StandardConfigurationControlsCM()
            {
                Controls = fields
            };

            var crateControls = _crate.Create(
                "Configuration_Controls",
                JsonConvert.SerializeObject(controls),
                CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME
            );

            return crateControls;
        }
    }
}