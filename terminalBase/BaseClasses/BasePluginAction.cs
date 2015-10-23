using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Core.Services;
using Data.Constants;
using Newtonsoft.Json;

using StructureMap;
using Core.Enums;
using Core.Interfaces;
using Core.Managers;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using Utilities.Configuration.Azure;
using TerminalBase.Infrastructure;
using Data.Interfaces;

namespace TerminalBase.BaseClasses
{
    //this method allows a specific Action to inject its own evaluation function into the 
    //standard ProcessConfigurationRequest
    public delegate ConfigurationRequestType ConfigurationEvaluator(ActionDTO curActionDTO);

    public class BasePluginAction
    {
        #region Fields

        protected IAction Action;
        protected ICrateManager Crate;
        protected IRouteNode Activity;
        private readonly Authorization _authorizationToken;
        private readonly IPlugin _plugin;
        #endregion

        public BasePluginAction()
        {
            Crate = ObjectFactory.GetInstance<ICrateManager>();
            Action = ObjectFactory.GetInstance<IAction>();
            Activity = ObjectFactory.GetInstance<IRouteNode>();
            _plugin = ObjectFactory.GetInstance<IPlugin>();
            _authorizationToken = new Authorization();
        }

        protected bool NeedsAuthentication(ActionDTO actionDTO)
        {
            if (actionDTO == null
                || actionDTO.AuthToken == null
                || string.IsNullOrEmpty(actionDTO.AuthToken.Token))
            {
                return true;
            }
        
            return false;
        }

        protected async Task<PayloadDTO> GetProcessPayload(int processId)
        {
            var httpClient = new HttpClient();
            var url = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                + "api/containers/"
                + processId.ToString();

            using (var response = await httpClient.GetAsync(url))
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<PayloadDTO>(content);
            }
        }

        protected async Task<CrateDTO> ValidateFields(List<FieldValidationDTO> requiredFieldList)
        {
            var httpClient = new HttpClient();

            var url = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                      + "field/exists";
            using (var response = await httpClient.PostAsJsonAsync(url, requiredFieldList))
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<List<FieldValidationResult>>(content);
                //do something with result
                //Crate.CreateDesignTimeFieldsCrate("ValidationErrors",)
                var validationErrorList = new List<FieldDTO>();
                //lets create necessary validationError crates
                for (var i = 0; i < result.Count; i++)
                {
                    var fieldCheckResult = result[i];
                    if (fieldCheckResult == FieldValidationResult.NotExists)
                    {
                        validationErrorList.Add(new FieldDTO() { Key = requiredFieldList[i].FieldName, Value = "Required"});
                    }
                }

                if (validationErrorList.Any())
                {
                    return Crate.CreateDesignTimeFieldsCrate("ValidationErrors", validationErrorList.ToArray());
                }
            }

            return null;
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

        //wrapper for support test method
        protected async virtual Task<List<CrateDTO>> GetCratesByDirection(int activityId, string manifestType, GetCrateDirection direction)
        {
            return await Activity.GetCratesByDirection(activityId, manifestType, direction);
        }

        public async Task<StandardDesignTimeFieldsCM> GetDesignTimeFields(
            int activityId, GetCrateDirection direction)
        {

            //1) Build a merged list of the upstream design fields to go into our drop down list boxes
            StandardDesignTimeFieldsCM mergedFields = new StandardDesignTimeFieldsCM();

            List<CrateDTO> curCrates = await Activity.GetCratesByDirection(
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
            var controlsCrate = Crate.CreateStandardConfigurationControlsCrate(
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
            return await Activity.GetCratesByDirection(curActionId, CrateManifests.STANDARD_FILE_HANDLE_MANIFEST_NAME, GetCrateDirection.Upstream);
        }

        protected async Task<CrateDTO> MergeUpstreamFields(int curActionDOId, string label)
        {
            var curUpstreamFields = (await GetDesignTimeFields(curActionDOId, GetCrateDirection.Upstream)).Fields.ToArray();
            CrateDTO upstreamFieldsCrate = Crate.CreateDesignTimeFieldsCrate(label, curUpstreamFields);

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

            var keys = Action.FindKeysByCrateManifestType(curActionDO, manifestSchema, fieldName).Result
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

        protected CrateDTO PackCrate_ErrorTextBox(string fieldLabel, string errorMessage)
        {
            ControlDefinitionDTO[] controls =
            {
                new TextBlockControlDefinitionDTO()
                {
                    Label = fieldLabel,
                    Value = errorMessage,
                    CssClass = "well well-lg"

                }
            };

            var crateControls = Crate.CreateStandardConfigurationControlsCrate(
                        "Configuration_Controls", controls
                    );

            return crateControls;
        }

        /// <summary>
        /// Returning the crate with text field control 
        /// </summary>
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

            var crateControls = Crate.Create(
                "Configuration_Controls",
                JsonConvert.SerializeObject(controls),
                CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME
            );

            return crateControls;
        }

        /// <summary>
        /// Creates RadioButtonGroup to enter specific value or choose value from upstream crate.
        /// </summary>
        protected ControlDefinitionDTO CreateSpecificOrUpstreamValueChooser(
            string label, string controlName, string upstreamSourceLabel)
        {
            var control = new RadioButtonGroupControlDefinitionDTO()
            {
                Label = label,
                GroupName = controlName,
                Name = controlName,
                Radios = new List<RadioButtonOption>()
                {
                    new RadioButtonOption()
                    {
                        Selected = true,
                        Name = "specific",
                        Value = "this specific value",
                        Controls = new List<ControlDefinitionDTO>()
                        {
                            new TextBoxControlDefinitionDTO()
                            {
                                Label = "",
                                Name = "SpecificValue"
                            }
                        }
                    },

                    new RadioButtonOption()
                    {
                        Selected = false,
                        Name = "upstream",
                        Value = "a value from an Upstream Crate",
                        Controls = new List<ControlDefinitionDTO>()
                        {
                            new DropDownListControlDefinitionDTO()
                            {
                                Label = "",
                                Name = "UpstreamCrate",
                                Source = new FieldSourceDTO
                                {
                                    Label = upstreamSourceLabel,
                                    ManifestType = CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME
                                }
                            }
                        }
                    }
                }
            };

            return control;
        }

        /// <summary>
        /// Extract value from RadioButtonGroup where specific value or upstream field was specified.
        /// </summary>
        protected string ExtractSpecificOrUpstreamValue(
            CrateStorageDTO designTimeCrateStorage,
            CrateStorageDTO runTimeCrateStorage,
            string controlName)
        {
            var controlsCrate = designTimeCrateStorage.CrateDTO.FirstOrDefault(
                c => c.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME);

            var controls = Crate.GetStandardConfigurationControls(controlsCrate).Controls;
            var radioButtonGroupControl = controls
                .SingleOrDefault(c => c.Name == controlName) as RadioButtonGroupControlDefinitionDTO;

            if (radioButtonGroupControl == null)
            {
                throw new ApplicationException("No Radio ButtonGroupControl found.");
            }

            var radioButton = radioButtonGroupControl
                .Radios
                .FirstOrDefault(x => x.Selected);

            if (radioButton == null)
            {
                throw new ApplicationException("radioButton == null;");
            }

            var returnValue = string.Empty;

            switch (radioButton.Name)
            {
                case "specific":
                    returnValue = radioButton.Controls[0].Value;
                    break;

                case "upstream":
                    var recipientField = radioButton.Controls[0];
                    returnValue = ExtractDesignTimeFieldValue(runTimeCrateStorage, radioButton.Controls[0].Value);
                    break;

                default:
                    throw new ApplicationException("Could not extract recipient, unknown recipient mode.");
            }

            return returnValue;
        }

        /// <summary>
        /// Extracts crate with specified label and ManifestType = Standard Design Time,
        /// then extracts field with specified fieldKey.
        /// </summary>
        protected string ExtractDesignTimeFieldValue(
            CrateStorageDTO crateStorage,
            string fieldKey)
        {
            var crates = Crate.GetCratesByManifestType(
                CrateManifests.STANDARD_PAYLOAD_MANIFEST_NAME, crateStorage);

            foreach (var crate in crates)
            {
                var allFields = JsonConvert.DeserializeObject<List<FieldDTO>>(crate.Contents);
                var searchField = allFields.FirstOrDefault(x => x.Key == fieldKey);

                if (searchField != null)
                {
                    return searchField.Value;
                }
            }

            throw new ApplicationException("No field found with specified key.");
        }
    }
}