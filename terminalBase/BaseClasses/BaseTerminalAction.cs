using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StructureMap;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Interfaces;
using Hub.Managers;
using Utilities.Configuration.Azure;
using TerminalBase.Infrastructure;

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

        private static HashSet<CrateManifestType> ExcludedManifestTypes = new HashSet<CrateManifestType>()
        {
            ManifestDiscovery.Default.GetManifestType<StandardConfigurationControlsCM>(),
            ManifestDiscovery.Default.GetManifestType<EventSubscriptionCM>()
        };

        public BaseTerminalAction()
        {
            Crate = new CrateManager();
            Action = ObjectFactory.GetInstance<IAction>();
            _terminal = ObjectFactory.GetInstance<ITerminal>();

            HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>();
        }

        /// <summary>
        /// Creates a suspend request for hub execution
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        protected PayloadDTO SuspendHubExecution(PayloadDTO payload)
        {
            using (var updater = Crate.UpdateStorage(payload))
            {
                var operationalState = updater.CrateStorage.CrateContentsOfType<OperationalStateCM>().Single();
                operationalState.CurrentActionResponse = ActionResponse.RequestSuspend;
            }

            return payload;
        }

        /// <summary>
        /// Creates a terminate request for hub execution
        /// TODO: we could include a reason message with this request
        /// after that we could stop throwing exceptions on actions
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        protected PayloadDTO TerminateHubExecution(PayloadDTO payload)
        {
            using (var updater = Crate.UpdateStorage(payload))
            {
                var operationalState = updater.CrateStorage.CrateContentsOfType<OperationalStateCM>().Single();
                operationalState.CurrentActionResponse = ActionResponse.RequestTerminate;
            }

            return payload;
        }

        /// <summary>
        /// returns success to hub
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        protected PayloadDTO Success(PayloadDTO payload)
        {
            using (var updater = Crate.UpdateStorage(payload))
            {
                var operationalState = updater.CrateStorage.CrateContentsOfType<OperationalStateCM>().Single();
                operationalState.CurrentActionResponse = ActionResponse.Success;
            }

            return payload;
        }

        /// <summary>
        /// skips children of this action
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        protected PayloadDTO SkipChildren(PayloadDTO payload)
        {
            using (var updater = Crate.UpdateStorage(payload))
            {
                var operationalState = updater.CrateStorage.CrateContentsOfType<OperationalStateCM>().Single();
                operationalState.CurrentActionResponse = ActionResponse.SkipChildren;
            }

            return payload;
        }

        /// <summary>
        /// returns error to hub
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        protected PayloadDTO Error(PayloadDTO payload, string errorMessage = null, ActionErrorCode? errorCode = null)
        {
            using (var updater = Crate.UpdateStorage(payload))
            {
                var operationalState = updater.CrateStorage.CrateContentsOfType<OperationalStateCM>().Single();
                operationalState.CurrentActionResponse = ActionResponse.Error;
                operationalState.CurrentActionErrorCode = errorCode;
                operationalState.CurrentActionErrorMessage = errorMessage;
            }

            return payload;
        }

        /// <summary>
        /// returns Needs authentication error to hub
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        protected PayloadDTO NeedsAuthenticationError(PayloadDTO payload)
        {
            return Error(payload, "No AuthToken provided.", ActionErrorCode.NO_AUTH_TOKEN_PROVIDED);
        }

        /// <summary>
        /// Creates a reprocess child actions request
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        protected PayloadDTO ReProcessChildActions(PayloadDTO payload)
        {
            using (var updater = Crate.UpdateStorage(payload))
            {
                var operationalState = updater.CrateStorage.CrateContentsOfType<OperationalStateCM>().Single();
                operationalState.CurrentActionResponse = ActionResponse.ReProcessChildren;
            }

            return payload;
        }


        public virtual async Task<PayloadDTO> ChildrenExecuted(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            return Success(await GetPayload(curActionDO, containerId));
        }

        protected void CheckAuthentication(AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }
        }

        protected bool NeedsAuthentication(AuthorizationTokenDO authTokenDO)
        {
            return authTokenDO == null || string.IsNullOrEmpty(authTokenDO.Token);
        }

        protected async Task<PayloadDTO> GetPayload(ActionDO actionDO, Guid containerId)
        {
            return await HubCommunicator.GetPayload(actionDO, containerId);
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

        public virtual async Task<ActionDO> Activate(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
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

        //wrapper for support test method
        public async virtual Task<List<Crate>> GetCratesByDirection(ActionDO actionDO, CrateDirection direction)
        {
            return await HubCommunicator.GetCratesByDirection(actionDO, direction);
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

        public async virtual Task<List<CrateManifestType>> BuildUpstreamManifestList(ActionDO actionDO)
        {
            var upstreamCrates = await this.GetCratesByDirection<Data.Interfaces.Manifests.Manifest>(actionDO, CrateDirection.Upstream);
            return upstreamCrates.Where(x => !ExcludedManifestTypes.Contains(x.ManifestType)).Select(f => f.ManifestType).Distinct().ToList();
        }

        public async virtual Task<List<String>> BuildUpstreamCrateLabelList(ActionDO actionDO)
        {
            var curCrates = await this.GetCratesByDirection<Data.Interfaces.Manifests.Manifest>(actionDO, CrateDirection.Upstream);
            return curCrates.Where(x => !ExcludedManifestTypes.Contains(x.ManifestType)).Select(f => f.Label).Distinct().ToList();
        }

        public async virtual Task<Crate<StandardDesignTimeFieldsCM>> GetUpstreamManifestListCrate(ActionDO actionDO)
        {
            var manifestList = (await BuildUpstreamManifestList(actionDO));
            var fields = manifestList.Select(f => new FieldDTO(f.Id.ToString(), f.Type)).ToArray();

            return Crate.CreateDesignTimeFieldsCrate("Upstream Manifest Type List", fields);
        }

        public async virtual Task<Crate<StandardDesignTimeFieldsCM>> GetUpstreamCrateLabelListCrate(ActionDO actionDO)
        {
            var labelList = (await BuildUpstreamCrateLabelList(actionDO));
            var fields = labelList.Select(f => new FieldDTO(null, f)).ToArray();

            return Crate.CreateDesignTimeFieldsCrate("Upstream Crate Label List", fields);
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
                GenerateTextBlock(fieldLabel,errorMessage,"well well-lg")
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
           PayloadDTO payloadCrates,
           string controlName)
        {
            var designTimeCrateStorage = Crate.GetStorage(actionDO.CrateStorage);
            var runTimeCrateStorage = Crate.FromDto(payloadCrates.CrateStorage);

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
                GenerateTextBlock(label,text,"well well-lg",name)
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

        protected virtual async Task<Crate> MergeUpstreamFields<TManifest>(ActionDO curActionDO, string label)
        {
            List<Data.Crates.Crate<TManifest>> crates = null;

            try
            {
                //throws exception from test classes when it cannot call webservice
                crates = await GetCratesByDirection<TManifest>(curActionDO, CrateDirection.Upstream);
            }
            catch { }

            if (crates != null)
            {
                FieldDTO[] upstreamFields;
                Crate availableFieldsCrate = null;
                if (crates is List<Data.Crates.Crate<StandardDesignTimeFieldsCM>>)
                {
                    upstreamFields = (crates as List<Data.Crates.Crate<StandardDesignTimeFieldsCM>>).SelectMany(x => x.Content.Fields).ToArray();

                    availableFieldsCrate = 
                        Crate.CreateDesignTimeFieldsCrate(
                            label,
                            upstreamFields
                        );
                }

                return await Task.FromResult(availableFieldsCrate);
            }

            return await Task.FromResult<Crate>(null);
        }

        /// <summary>
        /// Creates TextBlock control and fills it with label, value and CssClass
        /// </summary>
        /// <param name="curLabel">Label</param>
        /// <param name="curValue">Value</param>
        /// <param name="curCssClass">Css Class</param>
        /// <param name="curName">Name</param>
        /// <returns>TextBlock control</returns>
        protected TextBlock GenerateTextBlock(string curLabel, string curValue, string curCssClass, string curName = "unnamed")
        {
            return new TextBlock
            {
                Name = curName,
                Label = curLabel,
                Value = curValue,
                CssClass = curCssClass
            };
        }
        /// <summary>
        /// Method to be used with Loop Action
        /// Is a helper method to decouple some of the GetCurrentElement Functionality
        /// </summary>
        /// <param name="operationalCrate">Crate of the OperationalStateCM</param>
        /// <param name="loopId">Integer that is equal to the Action.Id</param>
        /// <returns>Index or pointer of the current IEnumerable Object</returns>
        protected int GetLoopIndex(OperationalStateCM operationalCrate, string loopId)
        {
            var curLoop = operationalCrate.Loops.FirstOrDefault(l => l.Id.Equals(loopId.ToString()));
            if (curLoop == null)
            {
                throw new NullReferenceException("No Loop with the specified LoopId inside the provided OperationalStateCM crate");
            }
            var curIndex = curLoop.Index;
            return curIndex;
        }
        /// <summary>
        /// Trivial method to return element at specified index of the IEnumerable object.
        /// To be used with Loop Action.
        /// IMPORTANT: 
        /// 1) Index update is performed by Loop Action
        /// 2) Loop brake is preformed by Loop Action
        /// </summary>
        /// <param name="enumerableObject">Object of type IEnumerable</param>
        /// <param name="objectIndex">Integer that points to the element</param>
        /// <returns>Object of any type</returns>
        protected object GetCurrentElement(IEnumerable<object> enumerableObject, int objectIndex)
        {
            var curElement = enumerableObject.ElementAt(objectIndex);
            return curElement;
        }

    }
}