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
using AutoMapper;

namespace TerminalBase.BaseClasses
{
    //this method allows a specific Action to inject its own evaluation function into the 
    //standard ProcessConfigurationRequest
    public delegate ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO);

    public class BaseTerminalActivity
    {
        #region Fields

        protected IActivity Activity;
        protected ICrateManager Crate;
        private readonly ITerminal _terminal;
        protected static readonly string ConfigurationControlsLabel = "Configuration_Controls";
        protected string CurrentFr8UserId { get; set; }

        public IHubCommunicator HubCommunicator { get; set; }
        #endregion

        private static HashSet<CrateManifestType> ExcludedManifestTypes = new HashSet<CrateManifestType>()
        {
            ManifestDiscovery.Default.GetManifestType<StandardConfigurationControlsCM>(),
            ManifestDiscovery.Default.GetManifestType<EventSubscriptionCM>()
        };

        public BaseTerminalActivity()
        {
            Crate = new CrateManager();
            Activity = ObjectFactory.GetInstance<IActivity>();
            _terminal = ObjectFactory.GetInstance<ITerminal>();
            HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>();
        }

        public void SetCurrentUser(string userId)
        {
            CurrentFr8UserId = userId;
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
                operationalState.CurrentActivityResponse = ActivityResponse.RequestSuspend;
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
                operationalState.CurrentActivityResponse = ActivityResponse.RequestTerminate;
            }

            return payload;
        }

        /// <summary>
        /// returns success to hub
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        protected PayloadDTO Success(PayloadDTO payload, string message = "")
        {
            using (var updater = Crate.UpdateStorage(payload))
            {
                var operationalState = updater.CrateStorage.CrateContentsOfType<OperationalStateCM>().Single();
                operationalState.CurrentActivityResponse = ActivityResponse.Success;
                operationalState.ResponseMessageDTO = new ResponseMessageDTO() { Message = message };
            }

            return payload;
        }

        protected PayloadDTO ExecuteClientAction(PayloadDTO payload, string clientActionName)
        {
            using (var updater = Crate.UpdateStorage(payload))
            {
                var operationalState = updater.CrateStorage.CrateContentsOfType<OperationalStateCM>().Single();
                operationalState.CurrentActivityResponse = ActivityResponse.ExecuteClientAction;
                operationalState.CurrentClientActionName = clientActionName;
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
                operationalState.CurrentActivityResponse = ActivityResponse.SkipChildren;
            }

            return payload;
        }

        /// <summary>
        /// returns error to hub
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="errorMessage"></param>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        protected PayloadDTO Error(PayloadDTO payload, string errorMessage = null, ActionErrorCode? errorCode = null)
        {
            using (var updater = Crate.UpdateStorage(payload))
            {
                var operationalState = updater.CrateStorage.CrateContentsOfType<OperationalStateCM>().Single();
                operationalState.CurrentActivityResponse = ActivityResponse.Error;
                operationalState.CurrentActivityErrorCode = errorCode;
                operationalState.CurrentActivityErrorMessage = errorMessage;
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
                operationalState.CurrentActivityResponse = ActivityResponse.ReProcessChildren;
            }

            return payload;
        }

        protected ControlDefinitionDTO GetControl(StandardConfigurationControlsCM controls, string name, string controlType = null)
        {
            Func<ControlDefinitionDTO, bool> predicate = x => x.Name == name;
            if (controlType != null)
            {
                predicate = x => x.Type == controlType && x.Name == name;
            }

            return controls.Controls.FirstOrDefault(predicate);
        }

        protected ControlDefinitionDTO GetControl(ActivityDO curActivityDO, string name, string controlType = null)
        {
            var controls = GetConfigurationControls(curActivityDO);
            return GetControl(controls, name, controlType);
        }

        protected StandardConfigurationControlsCM GetConfigurationControls(ActivityDO curActivityDO)
        {
            var storage = Crate.GetStorage(curActivityDO);
            return GetConfigurationControls(storage);
        }

        protected StandardConfigurationControlsCM GetConfigurationControls(CrateStorage storage)
        {
            var controlsCrate = storage.CrateContentsOfType<StandardConfigurationControlsCM>(c => c.Label == ConfigurationControlsLabel).FirstOrDefault();
            return controlsCrate;
        }


        public virtual async Task<PayloadDTO> ChildrenExecuted(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            return Success(await GetPayload(curActivityDO, containerId));
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

        protected async Task<PayloadDTO> GetPayload(ActivityDO activityDO, Guid containerId)
        {
            return await HubCommunicator.GetPayload(activityDO, containerId, CurrentFr8UserId);
        }

        protected async Task<Crate> ValidateFields(List<FieldValidationDTO> requiredFieldList)
        {
            var result = await HubCommunicator.ValidateFields(requiredFieldList, CurrentFr8UserId);

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

            return null;
        }

        protected async Task<CrateDTO> ValidateByStandartDesignTimeFields(ActivityDO curActivityDO, StandardDesignTimeFieldsCM designTimeFields)
        {
            var fields = designTimeFields.Fields;
            var validationList = fields.Select(f => new FieldValidationDTO(curActivityDO.Id, f.Key)).ToList();
            return Crate.ToDto(await ValidateFields(validationList));
        }

        //if the Action doesn't provide a specific method to override this, we just return null = no validation errors
        protected virtual async Task<CrateStorage> ValidateActivity(ActivityDO curActivityDO)
        {
            return await Task.FromResult<CrateStorage>(null);
        }

        protected async Task<ActivityDO> ProcessConfigurationRequest(ActivityDO curActivityDO, ConfigurationEvaluator configurationEvaluationResult, AuthorizationTokenDO authToken)
        {
            if (configurationEvaluationResult(curActivityDO) == ConfigurationRequestType.Initial)
            {
                return await InitialConfigurationResponse(curActivityDO, authToken);
            }

            else if (configurationEvaluationResult(curActivityDO) == ConfigurationRequestType.Followup)
            {
                var validationErrors = await ValidateActivity(curActivityDO);
                if (validationErrors != null)
                {
                    Crate.UpdateStorage(curActivityDO).CrateStorage.AddRange(validationErrors);
                    return curActivityDO;
                }
                return await FollowupConfigurationResponse(curActivityDO, authToken);
            }

            throw new InvalidDataException("Activity's Configuration Store does not contain connection_string field.");
        }

        /// <summary>
        /// Configure infrastructure.
        /// </summary>
        public virtual async Task<ActivityDO> Configure(ActivityDO activityDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(activityDO, ConfigurationEvaluator, authTokenDO);
        }

        /// <summary>
        /// This method "evaluates" as to what configuration should be called. 
        /// Every terminal action will have its own decision making; hence this method must be implemented in the relevant child class.
        /// </summary>
        /// <param name="curActivityDO"></param>
        /// <returns></returns>
        public virtual ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            throw new NotImplementedException("ConfigurationEvaluator method not implemented in child class.");
        }

        //if the Action doesn't provide a specific method to override this, we just return the existing CrateStorage, unchanged
        protected virtual async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            //Returns Task<ActivityDTO> using FromResult as the return type is known
            return await Task.FromResult<ActivityDO>(curActivityDO);
        }

        public virtual async Task<ActivityDO> Activate(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            //Returns Task<ActivityDTO> using FromResult as the return type is known
            var validationErrors = await ValidateActivity(curActivityDO);
            if (validationErrors != null)
            {
                Crate.UpdateStorage(curActivityDO).CrateStorage.AddRange(validationErrors);
                return curActivityDO;
            }
            return await Task.FromResult<ActivityDO>(curActivityDO);
        }

        public virtual async Task<ActivityDO> Deactivate(ActivityDO curActivityDO)
        {
            //Returns Task<ActivityDTO> using FromResult as the return type is known
            return await Task.FromResult<ActivityDO>(curActivityDO);
        }

        //if the Action doesn't provide a specific method to override this, we just return the existing CrateStorage, unchanged
        protected virtual async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            //Returns Task<ActivityDTO> using FromResult as the return type is known
            return await Task.FromResult<ActivityDO>(curActivityDO);
        }

        //wrapper for support test method
        public async virtual Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(ActivityDO activityDO, CrateDirection direction)
        {
            return await HubCommunicator.GetCratesByDirection<TManifest>(activityDO, direction, CurrentFr8UserId);
            // return await Activity.GetCratesByDirection<TManifest>(activityId, direction);
        }

        //wrapper for support test method
        public async virtual Task<List<Crate>> GetCratesByDirection(ActivityDO activityDO, CrateDirection direction)
        {
            return await HubCommunicator.GetCratesByDirection(activityDO, direction, CurrentFr8UserId);
        }

        public async virtual Task<StandardDesignTimeFieldsCM> GetDesignTimeFields(Guid activityId, CrateDirection direction, AvailabilityType availability = AvailabilityType.NotSet)
        {
            var mergedFields = await HubCommunicator.GetDesignTimeFieldsByDirection(activityId, direction, availability, CurrentFr8UserId);
            return mergedFields;
        }

        public async virtual Task<StandardDesignTimeFieldsCM> GetDesignTimeFields(ActivityDO activityDO, CrateDirection direction, AvailabilityType availability = AvailabilityType.NotSet)
        {
            var mergedFields = await HubCommunicator.GetDesignTimeFieldsByDirection(activityDO, direction, availability, CurrentFr8UserId);
            return mergedFields;
        }

        public async virtual Task<List<CrateManifestType>> BuildUpstreamManifestList(ActivityDO activityDO)
        {
            var upstreamCrates = await this.GetCratesByDirection<Data.Interfaces.Manifests.Manifest>(activityDO, CrateDirection.Upstream);
            return upstreamCrates.Where(x => !ExcludedManifestTypes.Contains(x.ManifestType)).Select(f => f.ManifestType).Distinct().ToList();
        }

        public async virtual Task<List<String>> BuildUpstreamCrateLabelList(ActivityDO activityDO)
        {
            var curCrates = await this.GetCratesByDirection<Data.Interfaces.Manifests.Manifest>(activityDO, CrateDirection.Upstream);
            return curCrates.Where(x => !ExcludedManifestTypes.Contains(x.ManifestType)).Select(f => f.Label).Distinct().ToList();
        }

        public async virtual Task<Crate<StandardDesignTimeFieldsCM>> GetUpstreamManifestListCrate(ActivityDO activityDO)
        {
            var manifestList = (await BuildUpstreamManifestList(activityDO));
            var fields = manifestList.Select(f => new FieldDTO(f.Id.ToString(), f.Type)).ToArray();

            return Crate.CreateDesignTimeFieldsCrate("Upstream Manifest Type List", fields);
        }

        public async virtual Task<Crate<StandardDesignTimeFieldsCM>> GetUpstreamCrateLabelListCrate(ActivityDO activityDO)
        {
            var labelList = (await BuildUpstreamCrateLabelList(activityDO));
            var fields = labelList.Select(f => new FieldDTO(null, f)).ToArray();

            return Crate.CreateDesignTimeFieldsCrate("Upstream Crate Label List", fields);
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
            return Crate<StandardConfigurationControlsCM>.FromContent(ConfigurationControlsLabel, new StandardConfigurationControlsCM(controlsList), AvailabilityType.Configuration);
        }

        protected string ExtractControlFieldValue(ActivityDO curActivityDO, string fieldName)
        {
            var storage = Crate.GetStorage(curActivityDO);

            var controlsCrateMS = storage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            //            var controlsCrate = curActivityDto.CrateStorage.CrateDTO
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

        protected async virtual Task<List<Crate<StandardFileDescriptionCM>>> GetUpstreamFileHandleCrates(ActivityDO activityDO)
        {
            return await HubCommunicator.GetCratesByDirection<StandardFileDescriptionCM>(activityDO, CrateDirection.Upstream, CurrentFr8UserId);
        }

        protected async Task<Crate<StandardDesignTimeFieldsCM>> MergeUpstreamFields(ActivityDO activityDO, string label)
        {
            var curUpstreamFields = (await GetDesignTimeFields(activityDO.Id, CrateDirection.Upstream)).Fields.ToArray();
            var upstreamFieldsCrate = Crate.CreateDesignTimeFieldsCrate(label, curUpstreamFields);

            return upstreamFieldsCrate;
        }

        /*protected ConfigurationRequestType ReturnInitialUnlessExistsField(ActionDTO curActionDTO, string fieldName, Data.Interfaces.Manifests.Manifest curSchema)
        {
            //CrateStorageDTO curCrates = curActionDTO.CrateStorage;
            var stroage = Crate.GetStorage(curActionDTO.CrateStorage);

            if (stroage.Count == 0)
                return ConfigurationRequestType.Initial;

            ActionDO curActivityDO = Mapper.Map<ActionDO>(curActionDTO);

            //load configuration crates of manifest type Standard Control Crates
            //look for a text field name select_file with a value
            Data.Interfaces.Manifests.Manifest manifestSchema = new Data.Interfaces.Manifests.Manifest(Data.Constants.MT.StandardConfigurationControls);

            var keys = Action.FindKeysByCrateManifestType(curActivityDO, manifestSchema, fieldName).Result
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

        /// <summary>
        /// Creates a crate with available design-time fields.
        /// </summary>
        /// <param name="activityDO">ActionDO.</param>
        /// <returns></returns>
        protected async Task<Crate> CreateAvailableFieldsCrate(ActivityDO activityDO, string crateLabel = "Upstream Terminal-Provided Fields")
        {
            var curUpstreamFields = await HubCommunicator.GetDesignTimeFieldsByDirection(activityDO, CrateDirection.Upstream, AvailabilityType.RunTime, CurrentFr8UserId);

            if (curUpstreamFields == null)
            {
                curUpstreamFields = new StandardDesignTimeFieldsCM();
            }

            var availableFieldsCrate = Crate.CreateDesignTimeFieldsCrate(
                    crateLabel,
                    curUpstreamFields.Fields,
                    AvailabilityType.Configuration
                );

            return availableFieldsCrate;
        }

        protected Crate PackCrate_ErrorTextBox(string fieldLabel, string errorMessage)
        {
            ControlDefinitionDTO[] controls =
            {
                GenerateTextBlock(fieldLabel,errorMessage,"well well-lg")
            };

            var crateControls = Crate.CreateStandardConfigurationControlsCrate(
                        ConfigurationControlsLabel, controls
                    );

            return crateControls;
        }

        /// <summary>
        /// Creates StandardConfigurationControlsCM with TextSource control
        /// </summary>
        /// <param name="storage">Crate Storage</param>
        /// <param name="label">Initial Label for the text source control</param>
        /// <param name="controlName">Name of the text source control</param>
        /// <param name="upstreamSourceLabel">Label for the upstream source</param>
        /// <param name="filterByTag">Filter for upstream source, Empty by default</param>
        /// <param name="addRequestConfigEvent">True if onChange event needs to be configured, False otherwise. True by default</param>
        /// <param name="required">True if the control is required, False otherwise. False by default</param>
        protected void AddTextSourceControl(CrateStorage storage, string label, string controlName,
                                            string upstreamSourceLabel, string filterByTag = "",
                                            bool addRequestConfigEvent = true, bool required = false)
        {
            var textSourceControl = CreateSpecificOrUpstreamValueChooser(label, controlName, upstreamSourceLabel,
                filterByTag, addRequestConfigEvent);
            textSourceControl.Required = required;

            AddControl(storage, textSourceControl);
        }

        /// <summary>
        /// Creates RadioButtonGroup to enter specific value or choose value from upstream crate.
        /// </summary>
        protected ControlDefinitionDTO CreateSpecificOrUpstreamValueChooser(
            string label, string controlName, string upstreamSourceLabel, string filterByTag = "", bool addRequestConfigEvent = true)
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
            if (addRequestConfigEvent)
            {
                control.Events.Add(new ControlEvent("onChange", "requestConfig"));
            }

            return control;
        }


        /// <summary>
        /// Extract value from RadioButtonGroup or TextSource where specific value or upstream field was specified.
        /// </summary>
        protected string ExtractSpecificOrUpstreamValue(ActivityDO activityDO, PayloadDTO payloadCrates, string controlName)
        {
            var designTimeCrateStorage = Crate.GetStorage(activityDO.CrateStorage);
            var runTimeCrateStorage = Crate.FromDto(payloadCrates.CrateStorage);

            var controls = designTimeCrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            var control = controls.Controls.SingleOrDefault(c => c.Name == controlName);

            if (control as RadioButtonGroup != null)
            {
                // Get value from a combination of RadioButtonGroup, TextField and DDLB controls
                // (old approach prior to TextSource) 
                return ExtractSpecificOrUpstreamValueLegacy((RadioButtonGroup)control, runTimeCrateStorage, activityDO);
            }

            if (control as TextSource == null)
            {
                throw new ApplicationException("TextSource control was expected but not found.");
            }

            var textSourceControl = (TextSource)control;

            switch (textSourceControl.ValueSource)
            {
                case "specific":
                    return textSourceControl.TextValue;

                case "upstream":
                    return ExtractPayloadFieldValue(runTimeCrateStorage, textSourceControl.selectedKey, activityDO);

                default:
                    throw new ApplicationException("Could not extract recipient, unknown recipient mode.");
            }
        }

        private string ExtractSpecificOrUpstreamValueLegacy(RadioButtonGroup radioButtonGroupControl, CrateStorage runTimeCrateStorage, ActivityDO curActivity)
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
                        returnValue = ExtractPayloadFieldValue(runTimeCrateStorage, radioButton.Controls[0].Value, curActivity);
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
        protected string ExtractPayloadFieldValue(CrateStorage crateStorage, string fieldKey, ActivityDO curActivity)
        {
            var fieldValues = crateStorage.CratesOfType<StandardPayloadDataCM>().SelectMany(x => x.Content.GetValues(fieldKey))
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();

            if (fieldValues.Length > 0)
                return fieldValues[0];

            IncidentReporter reporter = ObjectFactory.GetInstance<IncidentReporter>();
            reporter.IncidentMissingFieldInPayload(fieldKey, curActivity, "");

            throw new ApplicationException(string.Format("No field found with specified key: {0}.", fieldKey));
        }

        protected void AddLabelControl(CrateStorage storage, string name, string label, string text)
        {
            AddControl(
                storage,
                GenerateTextBlock(label, text, "well well-lg", name)
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


        /// <summary>
        /// specify selected FieldDO for DropDownList
        /// specify Value (TimeSpan) for Duration
        /// </summary>
        protected void SetControlValue(ActivityDO activity, string controlFullName, object value)
        {
            using (var updater = Crate.UpdateStorage(activity))
            {
                var controls = updater.CrateStorage
                    .CrateContentsOfType<StandardConfigurationControlsCM>()
                    .First().Controls;

                var control = TraverseNestedControls(controls, controlFullName);
                switch (control.Type)
                {
                    case "TextBlock":
                    case "TextBox":
                        control.Value = (string)value;
                        break;
                    case "CheckBox":
                        control.Selected = true;
                        break;
                    case "DropDownList":
                        var ddlb = control as DropDownList;
                        var val = value as ListItem;
                        ddlb.selectedKey = val.Key;
                        ddlb.Value = val.Value;
                        //ddlb.ListItems are not loaded yet
                        break;
                    case "Duration":
                        var duration = control as Duration;
                        var timespan = (TimeSpan)value;
                        duration.Days = timespan.Days;
                        duration.Hours = timespan.Hours;
                        duration.Minutes = timespan.Minutes;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private ControlDefinitionDTO TraverseNestedControls(List<ControlDefinitionDTO> controls, string childControl)
        {
            var controlNames = childControl.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            var control = controls.Where(a => a.Name == controlNames[0]).FirstOrDefault();
            if (controlNames.Count() == 1) return control;

            List<ControlDefinitionDTO> nestedControls = null;

            if (control.Type == "RadioButtonGroup")
            {
                var radio = (control as RadioButtonGroup).Radios.Where(a => a.Name == controlNames[1]).FirstOrDefault();
                radio.Selected = true;
                nestedControls = radio.Controls.ToList();
                return TraverseNestedControls(nestedControls, string.Join(".", controlNames.Skip(2)));
            }
            //TODO: Add support for future controls with nested child controls
            else
                throw new NotImplementedException("Can't search for controls inside of " + control.Type);
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
                controlsCrate = Crate.CreateStandardConfigurationControlsCrate(ConfigurationControlsLabel);
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

        protected virtual async Task<Crate> MergeUpstreamFields<TManifest>(ActivityDO curActivityDO, string label)
        {
            List<Data.Crates.Crate<TManifest>> crates = null;

            try
            {
                //throws exception from test classes when it cannot call webservice
                crates = await GetCratesByDirection<TManifest>(curActivityDO, CrateDirection.Upstream);
            }
            catch { }

            if (crates != null)
            {
                FieldDTO[] upstreamFields;
                Crate availableFieldsCrate = null;
                if (crates is List<Data.Crates.Crate<StandardDesignTimeFieldsCM>>)
                {
                    upstreamFields = (crates as List<Data.Crates.Crate<StandardDesignTimeFieldsCM>>).Where(w => w.Content.Fields.Where(x => x.Availability != AvailabilityType.Configuration).Count() == 1).SelectMany(x => x.Content.Fields).ToArray();

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

        protected virtual async Task<FieldDTO[]> GetCratesFieldsDTO<TManifest>(ActivityDO curActivityDO, CrateDirection crateDirection)
        {
            List<Data.Crates.Crate<TManifest>> crates = null;

            try
            {
                //throws exception from test classes when it cannot call webservice
                crates = await GetCratesByDirection<TManifest>(curActivityDO, crateDirection);
            }
            catch { }

            if (crates != null)
            {
                FieldDTO[] upstreamFields = null;
                if (crates is List<Data.Crates.Crate<StandardDesignTimeFieldsCM>>)
                {
                    upstreamFields = (crates as List<Data.Crates.Crate<StandardDesignTimeFieldsCM>>).Where(w => w.Content.Fields.Where(x => x.Availability != AvailabilityType.Configuration).Count() == 1).SelectMany(x => x.Content.Fields).ToArray();
                }

                return await Task.FromResult(upstreamFields);
            }

            return await Task.FromResult<FieldDTO[]>(null);
        }

        protected async Task<ActivityDO> AddAndConfigureChildActivity(ActivityDO parent, string templateName_Or_templateID, string name = null, string label = null, int order = -1)
        {

            //search activity template by name or id
            var allActivityTemplates = await HubCommunicator.GetActivityTemplates(parent, CurrentFr8UserId);
            int templateId;
            var activityTemplate = Int32.TryParse(templateName_Or_templateID, out templateId) ?
                allActivityTemplates.FirstOrDefault(a => a.Id == templateId)
                : allActivityTemplates.FirstOrDefault(a => a.Name == templateName_Or_templateID);

            if (activityTemplate == null) throw new Exception(string.Format("ActivityTemplate {0} was not found", templateName_Or_templateID));

            //assign missing properties
            label = string.IsNullOrEmpty(label) ? activityTemplate.Label : label;
            name = string.IsNullOrEmpty(name) ? activityTemplate.Label : label;

            var result = await HubCommunicator.CreateAndConfigureActivity(activityTemplate.Id, name, CurrentFr8UserId, label, parent.Id, order: order);
            var resultDO = Mapper.Map<ActivityDO>(result);

            if (resultDO != null)
            {
                parent.ChildNodes.Add(resultDO);
                return resultDO;
            }

            return null;
        }

        protected virtual Crate MergeUpstreamFields<TManifest>(ActivityDO curActivityDO, string label, FieldDTO[] upstreamFields)
        {
            if (upstreamFields != null)
            {
                var availableFieldsCrate =
                        Crate.CreateDesignTimeFieldsCrate(
                            label,
                            upstreamFields
                        );

                return availableFieldsCrate;
            }

            return null;
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