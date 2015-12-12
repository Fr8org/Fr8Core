using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Newtonsoft.Json;
using StructureMap;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;

using Hub.Interfaces;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Hub.Services;
using Utilities.Configuration.Azure;
using TerminalBase.Infrastructure;
using Data.Infrastructure;

namespace TerminalBase.BaseClasses
{
    //this method allows a specific Action to inject its own evaluation function into the 
    //standard ProcessConfigurationRequest
    public delegate ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO);

    public class BaseTerminalAction
    {
        #region Fields

        protected IAction Action;
        protected ICrateManager Crate;
        private readonly ITerminal _terminal;

        public IHubCommunicator HubCommunicator { get; set; }
        #endregion

        public BaseTerminalAction()
        {
            Crate = new CrateManager();
            Action = ObjectFactory.GetInstance<IAction>();
            _terminal = ObjectFactory.GetInstance<ITerminal>();

            HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>();
        }

        protected bool NeedsAuthentication(AuthorizationTokenDO authTokenDO)
        {
            if (authTokenDO == null
                || string.IsNullOrEmpty(authTokenDO.Token))
            {
                return true;
            }

            return false;
        }

        protected async Task<PayloadDTO> GetProcessPayload(ActionDO actionDO, Guid containerId)
        {
            return await HubCommunicator.GetProcessPayload(actionDO, containerId);
        }

        protected async Task<Crate> ValidateFields(List<FieldValidationDTO> requiredFieldList)
        {
            var httpClient = new HttpClient();

            var url = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                      + "api/" + CloudConfigurationManager.GetSetting("HubApiVersion") + "/field/exists";
            using (var response = await httpClient.PostAsJsonAsync(url, requiredFieldList))
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<List<FieldValidationResult>>(content);
                var validationErrorList = new List<FieldDTO>();
                //lets create necessary validationError crates
                for (var i = 0; i < result.Count; i++)
                {
                    var fieldCheckResult = result[i];
                    if (fieldCheckResult == FieldValidationResult.NotExists)
                    {
                        validationErrorList.Add(new FieldDTO() { Key = requiredFieldList[i].FieldName, Value = "Required" });
                    }
                }

                if (validationErrorList.Any())
                {
                    return Crate.CreateDesignTimeFieldsCrate("Validation Errors", validationErrorList.ToArray());
                }
            }

            return null;
        }

        protected async Task<IEnumerable<ActivityTemplateDO>> GetActivityTemplates(string tag = null)
        {
            var httpClient = new HttpClient();
            var url = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
            + "api/" + CloudConfigurationManager.GetSetting("HubApiVersion") + "/routenodes/available?tag=";

            if (string.IsNullOrEmpty(tag))
            {
                url += "[all]";
            }
            else
            {
                url += tag;
            }

            using (var response = await httpClient.GetAsync(url).ConfigureAwait(false))
            {
                var content = await response.Content.ReadAsStringAsync();
                var curActivityTemplate = JsonConvert.DeserializeObject<List<ActivityTemplateDTO>>(content);
                return curActivityTemplate.Select(at => Mapper.Map<ActivityTemplateDO>(at));
            }
        }

        protected async Task<CrateDTO> ValidateByStandartDesignTimeFields(ActionDO curActionDO, StandardDesignTimeFieldsCM designTimeFields)
        {
            var fields = designTimeFields.Fields;
            var validationList = fields.Select(f => new FieldValidationDTO(curActionDO.Id, f.Key)).ToList();
            return Crate.ToDto(await ValidateFields(validationList));
        }

        //if the Action doesn't provide a specific method to override this, we just return null = no validation errors
        protected virtual async Task<CrateStorage> ValidateAction(ActionDO curActionDO)
        {
            return null;
        }

        protected async Task<ActionDO> ProcessConfigurationRequest(ActionDO curActionDO, ConfigurationEvaluator configurationEvaluationResult, AuthorizationTokenDO authToken)
        {
            if (configurationEvaluationResult(curActionDO) == ConfigurationRequestType.Initial)
            {
                return await InitialConfigurationResponse(curActionDO, authToken);
            }

            else if (configurationEvaluationResult(curActionDO) == ConfigurationRequestType.Followup)
            {
                var validationErrors = await ValidateAction(curActionDO);
                if (validationErrors != null)
                {
                    Crate.UpdateStorage(curActionDO).CrateStorage.AddRange(validationErrors);
                    return curActionDO;
                }
                return await FollowupConfigurationResponse(curActionDO, authToken);
            }

            throw new InvalidDataException("Action's Configuration Store does not contain connection_string field.");
        }

        /// <summary>
        /// Configure infrastructure.
        /// </summary>
        public virtual async Task<ActionDO> Configure(ActionDO actionDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(actionDO, ConfigurationEvaluator, authTokenDO);
        }

        /// <summary>
        /// This method "evaluates" as to what configuration should be called. 
        /// Every terminal action will have its own decision making; hence this method must be implemented in the relevant child class.
        /// </summary>
        /// <param name="curActionDO"></param>
        /// <returns></returns>
        public virtual ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            throw new NotImplementedException("ConfigurationEvaluator method not implemented in child class.");
        }

        //if the Action doesn't provide a specific method to override this, we just return the existing CrateStorage, unchanged
        protected virtual async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            //Returns Task<ActivityDTO> using FromResult as the return type is known
            return await Task.FromResult<ActionDO>(curActionDO);
        }

        public virtual async Task<ActionDO> Activate(ActionDO curActionDO)
        {
            //Returns Task<ActivityDTO> using FromResult as the return type is known
            return await Task.FromResult<ActionDO>(curActionDO);
        }

        public virtual async Task<ActionDO> Deactivate(ActionDO curActionDO)
        {
            //Returns Task<ActivityDTO> using FromResult as the return type is known
            return await Task.FromResult<ActionDO>(curActionDO);
        }

        //if the Action doesn't provide a specific method to override this, we just return the existing CrateStorage, unchanged
        protected virtual async Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            //Returns Task<ActivityDTO> using FromResult as the return type is known
            return await Task.FromResult<ActionDO>(curActionDO);
        }

        //wrapper for support test method
        public async virtual Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(
            ActionDO actionDO, CrateDirection direction)
        {
            return await HubCommunicator.GetCratesByDirection<TManifest>(actionDO, direction);
            // return await Activity.GetCratesByDirection<TManifest>(activityId, direction);
        }

        public async virtual Task<StandardDesignTimeFieldsCM> GetDesignTimeFields(ActionDO actionDO, CrateDirection direction)
        {
            //1) Build a merged list of the upstream design fields to go into our drop down list boxes
            StandardDesignTimeFieldsCM mergedFields = new StandardDesignTimeFieldsCM();

            var curCrates = await HubCommunicator
                .GetCratesByDirection<StandardDesignTimeFieldsCM>(actionDO, direction);


            mergedFields.Fields.AddRange(MergeContentFields(curCrates).Fields);

            return mergedFields;
        }

        public StandardDesignTimeFieldsCM MergeContentFields(List<Crate<StandardDesignTimeFieldsCM>> curCrates)
        {
            StandardDesignTimeFieldsCM tempMS = new StandardDesignTimeFieldsCM();
            foreach (var curCrate in curCrates)
            {
                //extract the fields
                StandardDesignTimeFieldsCM curStandardDesignTimeFieldsCrate = curCrate.Content;

                //add them to the pile
                tempMS.Fields.AddRange(curStandardDesignTimeFieldsCrate.Fields);
            }

            return tempMS;
        }

        protected CrateStorage AssembleCrateStorage(params Crate[] curCrates)
        {
            return new CrateStorage(curCrates);
        }

        protected Crate PackControls(StandardConfigurationControlsCM page)
        {
            return PackControlsCrate(page.Controls.ToArray());
        }

        protected Crate<StandardConfigurationControlsCM> PackControlsCrate(params ControlDefinitionDTO[] controlsList)
        {
            return Crate<StandardConfigurationControlsCM>.FromContent("Configuration_Controls", new StandardConfigurationControlsCM(controlsList));
        }

        protected string ExtractControlFieldValue(ActionDO curActionDO, string fieldName)
        {
            var storage = Crate.GetStorage(curActionDO);

            var controlsCrateMS = storage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            //            var controlsCrate = curActionDto.CrateStorage.CrateDTO
            //                .FirstOrDefault(
            //                    x => x.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME
            //                    && x.Label == "Configuration_Controls");
            //
            //            if (controlsCrate == null)
            //            {
            //                throw new ApplicationException("No Configuration_Controls crate found.");
            //            }
            //
            //            var controlsCrateMS = JsonConvert
            //                .DeserializeObject<StandardConfigurationControlsCM>(
            //                    controlsCrate.Contents
            //                );

            var field = controlsCrateMS.Controls
                .FirstOrDefault(x => x.Name == fieldName);

            if (field == null)
            {
                return null;
            }

            return field.Value;
        }

        protected async virtual Task<List<Crate<StandardFileHandleMS>>>
            GetUpstreamFileHandleCrates(ActionDO actionDO)
        {
            return await HubCommunicator
                .GetCratesByDirection<StandardFileHandleMS>(
                    actionDO, CrateDirection.Upstream
                );
        }

        protected async Task<Crate<StandardDesignTimeFieldsCM>>
            MergeUpstreamFields(ActionDO actionDO, string label)
        {
            var curUpstreamFields = (await GetDesignTimeFields(actionDO, CrateDirection.Upstream)).Fields.ToArray();
            var upstreamFieldsCrate = Crate.CreateDesignTimeFieldsCrate(label, curUpstreamFields);

            return upstreamFieldsCrate;
        }

        /*protected ConfigurationRequestType ReturnInitialUnlessExistsField(ActionDTO curActionDTO, string fieldName, Data.Interfaces.Manifests.Manifest curSchema)
        {
            //CrateStorageDTO curCrates = curActionDTO.CrateStorage;
            var stroage = Crate.GetStorage(curActionDTO.CrateStorage);

            if (stroage.Count == 0)
                return ConfigurationRequestType.Initial;

            ActionDO curActionDO = Mapper.Map<ActionDO>(curActionDTO);

            //load configuration crates of manifest type Standard Control Crates
            //look for a text field name select_file with a value
            Data.Interfaces.Manifests.Manifest manifestSchema = new Data.Interfaces.Manifests.Manifest(Data.Constants.MT.StandardConfigurationControls);

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

        }*/

        protected Crate PackCrate_ErrorTextBox(string fieldLabel, string errorMessage)
        {
            ControlDefinitionDTO[] controls =
            {
                new TextBlock()
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
        /// Creates RadioButtonGroup to enter specific value or choose value from upstream crate.
        /// </summary>
        protected ControlDefinitionDTO CreateSpecificOrUpstreamValueChooser(
            string label, string controlName, string upstreamSourceLabel, string filterByTag = "")
        {
            var control = new TextSource(label, upstreamSourceLabel, controlName)
            {
                Source = new FieldSourceDTO
                {
                    Label = upstreamSourceLabel,
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields,
                    FilterByTag = filterByTag
                }
            };

            return control;
        }


        /// <summary>
        /// Extract value from RadioButtonGroup or TextSource where specific value or upstream field was specified.
        /// </summary>
        protected string ExtractSpecificOrUpstreamValue(
           ActionDO actionDO,
           PayloadDTO processPayload,
           string controlName)
        {
            var designTimeCrateStorage = Crate.GetStorage(actionDO.CrateStorage);
            var runTimeCrateStorage = Crate.FromDto(processPayload.CrateStorage);

            var controls = designTimeCrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            var control = controls.Controls.SingleOrDefault(c => c.Name == controlName);

            if (control as RadioButtonGroup != null)
            {
                // Get value from a combination of RadioButtonGroup, TextField and DDLB controls
                // (old approach prior to TextSource) 
                return ExtractSpecificOrUpstreamValueLegacy((RadioButtonGroup)control, runTimeCrateStorage, actionDO);
            }

            if (control as TextSource == null)
            {
                throw new ApplicationException("TextSource control was expected but not found.");
            }

            TextSource textSourceControl = (TextSource)control;

            switch (textSourceControl.ValueSource)
            {
                case "specific":
                    return textSourceControl.Value;

                case "upstream":
                    return ExtractPayloadFieldValue(runTimeCrateStorage, textSourceControl.selectedKey, actionDO);

                default:
                    throw new ApplicationException("Could not extract recipient, unknown recipient mode.");
            }
        }

        private string ExtractSpecificOrUpstreamValueLegacy(RadioButtonGroup radioButtonGroupControl, CrateStorage runTimeCrateStorage, ActionDO curAction)
        {
            var radioButton = radioButtonGroupControl
                .Radios
                .FirstOrDefault(x => x.Selected);

            if (radioButton == null)
            {
                throw new ApplicationException("radioButton == null;");
            }

            var returnValue = string.Empty;

            try
            {
                switch (radioButton.Name)
                {
                    case "specific":
                        returnValue = radioButton.Controls[0].Value;
                        break;

                    case "upstream":
                        var recipientField = radioButton.Controls[0];
                        returnValue = ExtractPayloadFieldValue(runTimeCrateStorage, radioButton.Controls[0].Value, curAction);
                        break;

                    default:
                        throw new ApplicationException("Could not extract recipient, unknown recipient mode.");
                }
            }
            catch (ApplicationException)
            {

            }

            return returnValue;
        }

        /// <summary>
        /// Extracts crate with specified label and ManifestType = Standard Design Time,
        /// then extracts field with specified fieldKey.
        /// </summary>
        protected string ExtractPayloadFieldValue(CrateStorage crateStorage, string fieldKey, ActionDO curAction)
        {
            var fieldValues = crateStorage.CratesOfType<StandardPayloadDataCM>().SelectMany(x => x.Content.GetValues(fieldKey))
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();

            if (fieldValues.Length > 0)
                return fieldValues[0];

            IncidentReporter reporter = new IncidentReporter();
            reporter.IncidentMissingFieldInPayload(fieldKey, curAction, "");

            throw new ApplicationException("No field found with specified key.");
        }

        protected void AddLabelControl(CrateStorage storage, string name, string label, string text)
        {
            AddControl(
                storage,
                new TextBlock()
                {
                    Name = name,
                    Label = label,
                    Value = text,
                    CssClass = "well well-lg"
                }
            );
        }

        protected void AddControl(CrateStorage storage, ControlDefinitionDTO control)
        {
            var controlsCrate = EnsureControlsCrate(storage);

            if (controlsCrate.Content == null) { return; }

            controlsCrate.Content.Controls.Add(control);
        }

        protected ControlDefinitionDTO FindControl(CrateStorage storage, string name)
        {
            var controlsCrate = storage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (controlsCrate == null) { return null; }

            var control = controlsCrate.Controls
                .FirstOrDefault(x => x.Name == name);

            return control;
        }

        protected void RemoveControl(CrateStorage storage, string name)
        {
            var controlsCrate = storage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (controlsCrate == null) { return; }


            var control = controlsCrate.Controls.FirstOrDefault(x => x.Name == name);

            if (control != null)
            {
                controlsCrate.Controls.Remove(control);
            }
        }

        protected Crate<StandardConfigurationControlsCM> EnsureControlsCrate(CrateStorage storage)
        {
            var controlsCrate = storage.CratesOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (controlsCrate == null)
            {
                controlsCrate = Crate.CreateStandardConfigurationControlsCrate("Configuration_Controls");
                storage.Add(controlsCrate);
            }

            return controlsCrate;
        }

        protected void UpdateDesignTimeCrateValue(CrateStorage storage, string label, params FieldDTO[] fields)
        {
            var crate = storage.CratesOfType<StandardDesignTimeFieldsCM>().FirstOrDefault(x => x.Label == label);

            if (crate == null)
            {
                crate = Crate.CreateDesignTimeFieldsCrate(label, fields);

                storage.Add(crate);
            }
            else
            {
                crate.Content.Fields.Clear();
                crate.Content.Fields.AddRange(fields);
            }
        }
    }
}