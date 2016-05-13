using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StructureMap;
using TerminalBase.Infrastructure;
<<<<<<< HEAD
=======
using AutoMapper;
using Data.Interfaces.Manifests;
>>>>>>> refs/remotes/origin/dev
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.DataTransferObjects.Helpers;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using Fr8Data.States;
using TerminalBase.Errors;
using TerminalBase.Helpers;
using TerminalBase.Interfaces;
using TerminalBase.Models;
using TerminalBase.Services;

namespace TerminalBase.BaseClasses
{
    public abstract class BaseTerminalActivity : IActivity
    {
        protected BaseTerminalActivity(bool isAuthenticationRequired)
        {
            IsAuthenticationRequired = isAuthenticationRequired;
        }

        private void InitializeActivity(ActivityContext activityContext, ContainerExecutionContext containerExecutionContext = null)
        {
            ActivityContext = activityContext;
            ExecutionContext = containerExecutionContext;
            CrateManager = ObjectFactory.GetInstance<ICrateManager>();
            HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>();
            CrateSignaller = new CrateSignaller(Storage, MyTemplate.Name);
        }

        #region FIELDS

        private List<ActivityTemplateDTO> _activityTemplateCache = null;
        protected static readonly string ConfigurationControlsLabel = "Configuration_Controls";
        protected ICrateManager CrateManager { get; private set; }
        public IHubCommunicator HubCommunicator { get; set; }
        public CrateSignaller CrateSignaller { get; set; }
        protected ContainerExecutionContext ExecutionContext { get; set; }
        protected ActivityContext ActivityContext { get; set; }
        protected abstract ActivityTemplateDTO MyTemplate { get; }
        protected bool IsAuthenticationRequired { get; }


        #endregion

        #region SHORTCUTS
        private ControlHelper _controlHelper;
        private UpstreamQueryManager _upstreamQueryManager;
        private StandardConfigurationControlsCM _configurationControls;
        protected ICrateStorage Storage => ActivityContext.ActivityPayload.CrateStorage;
        protected AuthorizationToken AuthorizationToken => ActivityContext.AuthorizationToken;
        protected OperationalStateCM OperationalState => GetOperationalStateCrate(Payload);
        protected ICrateStorage Payload => ExecutionContext.PayloadStorage;
        protected StandardConfigurationControlsCM ConfigurationControls => _configurationControls ?? (_configurationControls = GetConfigurationControls());
        protected int LoopIndex => GetLoopIndex();
        protected UpstreamQueryManager UpstreamQueryManager => _upstreamQueryManager ?? (_upstreamQueryManager = new UpstreamQueryManager(ActivityContext, HubCommunicator));
        protected ControlHelper ControlHelper => _controlHelper ?? (_controlHelper = new ControlHelper(ActivityContext, HubCommunicator));
        protected Guid ActivityId => ActivityContext.ActivityPayload.Id;
        protected string CurrentUserId => ActivityContext.UserId;
        protected Task<ActivityPayload> SaveToHub(ActivityPayload activity) => HubCommunicator.SaveActivity(activity, CurrentUserId);
        public Task<FieldDescriptionsCM> GetDesignTimeFields(CrateDirection direction, AvailabilityType availability = AvailabilityType.NotSet) => HubCommunicator.GetDesignTimeFieldsByDirection(ActivityId, direction, availability, CurrentUserId);

        #endregion

        #region RETURN_CODES

        /// <summary>
        /// Creates a suspend request for hub execution
        /// </summary>
        /// <returns></returns>
        protected void SuspendHubExecution()
        {
            OperationalState.CurrentActivityResponse = ActivityResponseDTO.Create(ActivityResponse.RequestSuspend);
        }

        /// <summary>
        /// Creates a terminate request for hub execution
        /// TODO: we could include a reason message with this request
        /// after that we could stop throwing exceptions on actions
        /// </summary>
        /// <param name="message">Reason for termination</param>
        /// <returns></returns>
        protected void TerminateHubExecution(string message = null)
        {
            OperationalState.CurrentActivityResponse = ActivityResponseDTO.Create(ActivityResponse.RequestTerminate);
            OperationalState.CurrentActivityResponse.AddResponseMessageDTO(new ResponseMessageDTO { Message = message });
        }

        protected void LaunchPlan(Guid targetPlanId)
        {
            OperationalState.CurrentActivityResponse = ActivityResponseDTO.Create(ActivityResponse.LaunchAdditionalPlan);
            OperationalState.CurrentActivityResponse.AddResponseMessageDTO(new ResponseMessageDTO { Details = targetPlanId });
        }

        protected void JumpToSubplan(Guid targetSubplanId)
        {
            OperationalState.CurrentActivityResponse = ActivityResponseDTO.Create(ActivityResponse.JumpToSubplan);
            OperationalState.CurrentActivityResponse.AddResponseMessageDTO(new ResponseMessageDTO { Details = targetSubplanId });
        }

        /// <summary>
        /// Jumps to an activity that resides in same subplan as current activity
        /// </summary>
        /// <param name="targetActivityId"></param>
        /// <returns></returns>
        protected void JumpToActivity(Guid targetActivityId)
        {
            OperationalState.CurrentActivityResponse = ActivityResponseDTO.Create(ActivityResponse.JumpToActivity);
            OperationalState.CurrentActivityResponse.AddResponseMessageDTO(new ResponseMessageDTO { Details = targetActivityId });
        }

        /// <summary>
        /// Jumps to an activity that resides in same subplan as current activity
        /// </summary>
        /// <param name="targetPlanId"></param>
        /// <returns></returns>
        protected void LaunchAdditionalPlan(Guid targetPlanId)
        {
            OperationalState.CurrentActivityResponse = ActivityResponseDTO.Create(ActivityResponse.LaunchAdditionalPlan);
            OperationalState.CurrentActivityResponse.AddResponseMessageDTO(new ResponseMessageDTO { Details = targetPlanId });
        }

        /// <summary>
        /// returns success to hub
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected void Success(string message = "")
        {
            OperationalState.CurrentActivityResponse = ActivityResponseDTO.Create(ActivityResponse.Success);
            OperationalState.CurrentActivityResponse.AddResponseMessageDTO(new ResponseMessageDTO { Message = message });
        }

        protected void ExecuteClientActivity(string clientActivityName)
        {
            OperationalState.CurrentActivityResponse = ActivityResponseDTO.Create(ActivityResponse.ExecuteClientActivity);
            OperationalState.CurrentClientActivityName = clientActivityName;
        }

        /// <summary>
        /// skips children of this action
        /// </summary>
        /// <returns></returns>
        protected void SkipChildren()
        {
            OperationalState.CurrentActivityResponse = ActivityResponseDTO.Create(ActivityResponse.SkipChildren);
        }

        /// <summary>
        /// returns error to hub
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="errorMessage"></param>
        /// <param name="errorCode"></param>
        /// <param name="currentActivity">Activity where the error occured</param>
        /// <param name="currentTerminal">Terminal where the error occured</param>
        /// <returns></returns>
        protected void RaiseError(string errorMessage = null, ActivityErrorCode? errorCode = null, string currentActivity = null, string currentTerminal = null)
        {
            RaiseError(errorMessage, ErrorType.Generic, errorCode, currentActivity, currentTerminal);
        }

        /// <summary>
        /// returns error to hub
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="currentActivity">Activity where the error occured</param>
        /// <param name="currentTerminal">Terminal where the error occured</param>
        /// <param name="errorMessage"></param>
        /// <param name="errorType"></param>
        /// <returns></returns>
        protected void RaiseError(string errorMessage, ErrorType errorType, ActivityErrorCode? errorCode = null, string currentActivity = null, string currentTerminal = null)
        {
            OperationalState.CurrentActivityErrorCode = errorCode;
            OperationalState.CurrentActivityResponse = ActivityResponseDTO.Create(ActivityResponse.Error);
            OperationalState.CurrentActivityResponse.AddErrorDTO(ErrorDTO.Create(errorMessage, errorType, errorCode.ToString(), null, currentActivity, currentTerminal));
        }

        /// <summary>
        /// Returns Needs authentication error to hub
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        protected void RaiseNeedsAuthenticationError()
        {
            RaiseError("No AuthToken provided.", ErrorType.Authentication, ActivityErrorCode.AUTH_TOKEN_NOT_PROVIDED_OR_INVALID);
        }

        /// <summary>
        /// Returns authentication error to hub
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        protected void RaiseInvalidTokenError(string instructionsToUser = null)
        {
            RaiseError(instructionsToUser, ErrorType.Authentication, ActivityErrorCode.AUTH_TOKEN_NOT_PROVIDED_OR_INVALID);
        }

        #endregion

        protected bool CheckAuthentication()
        {
            if (NeedsAuthentication())
            {
                AddAuthenticationCrate(false);
                return true;
            }

            return false;
        }

        protected async Task PushUserNotification(string type, string subject, string message)
        {
            var notificationMsg = new TerminalNotificationDTO
            {
                Type = type,
                Subject = subject,
                Message = message,
                ActivityName = MyTemplate.Name,
                ActivityVersion = MyTemplate.Version,
                TerminalName = MyTemplate.Terminal.Name,
                TerminalVersion = MyTemplate.Terminal.Version
            };
            await HubCommunicator.NotifyUser(notificationMsg, CurrentUserId);
        }

        protected string ExtractPayloadFieldValue(string fieldKey)
        {
            var fieldValues = Payload.CratesOfType<StandardPayloadDataCM>().SelectMany(x => x.Content.GetValues(fieldKey))
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();
            if (fieldValues.Length > 0)
                return fieldValues[0];
            var baseEvent = new BaseTerminalEvent();
            var exceptionMessage = $"No field found with specified key: {fieldKey}.";
            //This is required for proper logging of the Incidents
            baseEvent.SendTerminalErrorIncident(MyTemplate.Terminal.Name, exceptionMessage, MyTemplate.Name, CurrentUserId);
            throw new ApplicationException(exceptionMessage);
        }

        protected void AddAuthenticationCrate(bool revocation)
        {
            var terminalAuthType = ActivityContext.ActivityPayload.ActivityTemplate.Terminal.AuthenticationType;

            AuthenticationMode mode;
            switch (terminalAuthType)
            {
                case AuthenticationType.Internal:
                    mode = AuthenticationMode.InternalMode;
                    break;
                case AuthenticationType.External:
                    mode = AuthenticationMode.ExternalMode;
                    break;
                case AuthenticationType.InternalWithDomain:
                    mode = AuthenticationMode.InternalModeWithDomain;
                    break;
                case AuthenticationType.None:
                    mode = AuthenticationMode.ExternalMode;
                    break;
                default:
                    throw new ActivityExecutionException("Unknown authentication type");
            }

            Storage.Add(
                CrateManager.CreateAuthenticationCrate("RequiresAuthentication", mode, revocation)
            );
        }

        public virtual bool NeedsAuthentication()
        {
            return string.IsNullOrEmpty(AuthorizationToken?.Token);
        }

        protected OperationalStateCM GetOperationalStateCrate(ICrateStorage storage)
        {
            var operationalState = storage.CrateContentsOfType<OperationalStateCM>().SingleOrDefault();
            if (operationalState == null)
                throw new Exception("No Operational State Crate found.");
            return operationalState;
        }

        /// <summary>
        /// Method to be used with Loop Action
        /// Is a helper method to decouple some of the GetCurrentElement Functionality
        /// </summary>
        /// <param name="operationalCrate">Crate of the OperationalStateCM</param>
        /// <returns>Index or pointer of the current IEnumerable Object</returns>
        protected int GetLoopIndex()
        {
            var loopState = OperationalState.CallStack.FirstOrDefault(x => x.LocalData?.Type == "Loop");
            if (loopState != null) //this is a loop related data request
            {
                return loopState.LocalData.ReadAs<OperationalStateCM.LoopStatus>().Index;
            }
            throw new NullReferenceException("No Loop was found inside the provided OperationalStateCM crate");
        }

        protected async Task<ActivityPayload> ConfigureChildActivity(ActivityPayload parent, ActivityPayload child)
        {
            var result = await HubCommunicator.ConfigureActivity(child, CurrentUserId);
            parent.ChildrenActivities.Remove(child);
            parent.ChildrenActivities.Add(result);
            return result;
        }

        protected async Task<Crate<FieldDescriptionsCM>> MergeUpstreamFields(string label)
        {
            var curUpstreamFields = (await GetDesignTimeFields(CrateDirection.Upstream)).Fields.ToArray();
            var upstreamFieldsCrate = CrateManager.CreateDesignTimeFieldsCrate(label, curUpstreamFields);
            return upstreamFieldsCrate;
        }
<<<<<<< HEAD

        protected virtual ConfigurationRequestType GetConfigurationRequestType()
        {
            return Storage.Count == 0 ? ConfigurationRequestType.Initial : ConfigurationRequestType.Followup;
        }

        protected Crate PackControls(StandardConfigurationControlsCM page)
        {
            return PackControlsCrate(page.Controls.ToArray());
        }

        protected Crate<StandardConfigurationControlsCM> PackControlsCrate(params ControlDefinitionDTO[] controlsList)
        {
            return Crate<StandardConfigurationControlsCM>.FromContent(ConfigurationControlsLabel, new StandardConfigurationControlsCM(controlsList), AvailabilityType.Configuration);
=======
       
        //if the Action doesn't provide a specific method to override this, we just return null = no validation errors
        public virtual Task ValidateActivity(ActivityDO activityDo, ICrateStorage currActivityCrateStorage, ValidationManager validationManager)
        {
            return Task.FromResult(0);
>>>>>>> refs/remotes/origin/dev
        }

        protected StandardConfigurationControlsCM GetConfigurationControls()
        {
<<<<<<< HEAD
            var controlsCrate = Storage.CrateContentsOfType<StandardConfigurationControlsCM>(c => c.Label == ConfigurationControlsLabel).FirstOrDefault();
            return controlsCrate;
        }

        protected T GetControl<T>(string name, string controlType = null) where T : ControlDefinitionDTO
        {
            Func<ControlDefinitionDTO, bool> predicate = x => x.Name == name;
            if (controlType != null)
            {
                predicate = x => x.Type == controlType && x.Name == name;
            }

            return (T)ConfigurationControls.Controls.FirstOrDefault(predicate);
        }

        private bool AuthorizeIfNecessary()
        {
            if (IsAuthenticationRequired)
            {
                return CheckAuthentication();
            }

            return false;
=======
            var configRequestType = configurationEvaluationResult(curActivityDO);

            if (configRequestType == ConfigurationRequestType.Initial)
            {
                return await InitialConfigurationResponse(curActivityDO, authToken);
            }

            if (configRequestType == ConfigurationRequestType.Followup)
            {
                using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
                {
                    crateStorage.Remove<ValidationResultsCM>();

                    var currentValidationResults = new ValidationResultsCM();
                    var validationManager = new ValidationManager(currentValidationResults);

                    await ValidateActivity(curActivityDO, crateStorage, validationManager);

                    if (validationManager.HasErrors)
                    {
                        crateStorage.Add(Crate.FromContent("Validation Results", currentValidationResults));
                        crateStorage.Flush();
                        return curActivityDO;
                    }
                }

                var result = await FollowupConfigurationResponse(curActivityDO, authToken);

                return result;
            }

            throw new NotSupportedException($"Unsupported ConfigurationRequestType: {configRequestType}");
        }
        
        /// <summary>
        /// Configure infrastructure.
        /// </summary>
        public virtual async Task<ActivityDO> Configure(ActivityDO activityDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(activityDO, ConfigurationEvaluator, authTokenDO);
>>>>>>> refs/remotes/origin/dev
        }

        protected void RemoveControl<T>(string name) where T : ControlDefinitionDTO
        {
            var control = GetControl<T>(name);
            if (control != null)
            {
                ConfigurationControls.Controls.Remove(control);
            }
        }

        protected void AddLabelControl(string name, string label, string text)
        {
            AddControl(ControlHelper.GenerateTextBlock(label, text, "well well-lg", name));
        }

        protected void AddControl(ControlDefinitionDTO control)
        {
<<<<<<< HEAD
            ConfigurationControls.Controls.Add(control);
=======
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Remove<ValidationResultsCM>();

                var currentValidationResults = new ValidationResultsCM();
                var validationManager = new ValidationManager(currentValidationResults);
                
                await ValidateActivity(curActivityDO, crateStorage, validationManager);

                if (validationManager.HasErrors)
                {
                    crateStorage.Add(Crate.FromContent("Validation errors", currentValidationResults));
                }
            }

            return await Task.FromResult<ActivityDO>(curActivityDO);
>>>>>>> refs/remotes/origin/dev
        }

        protected virtual bool IsTokenInvalidation(Exception ex)
        {
            return false;
        }

        protected async Task<ActivityPayload> AddAndConfigureChildActivity(Guid parentActivityId, ActivityTemplateDTO activityTemplate, string name = null, string label = null, int? order = null)
        {
            //assign missing properties
            label = string.IsNullOrEmpty(label) ? activityTemplate.Label : label;
            name = string.IsNullOrEmpty(name) ? activityTemplate.Label : label;
            return await HubCommunicator.CreateAndConfigureActivity(activityTemplate.Id, CurrentUserId, name, order, parentActivityId);
        }

        protected void UpdateDesignTimeCrateValue(string label, params FieldDTO[] fields)
        {
            var crate = Storage.CratesOfType<FieldDescriptionsCM>().FirstOrDefault(x => x.Label == label);

            if (crate == null)
            {
                crate = CrateManager.CreateDesignTimeFieldsCrate(label, fields);
                Storage.Add(crate);
            }
            else
            {
                crate.Content.Fields.Clear();
                crate.Content.Fields.AddRange(fields);
            }
        }

        //wrapper for support test method
        public virtual async Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(CrateDirection direction)
        {
            return await HubCommunicator.GetCratesByDirection<TManifest>(ActivityId, direction, CurrentUserId);
        }

        //wrapper for support test method
        public virtual async Task<List<Crate>> GetCratesByDirection(CrateDirection direction)
        {
            return await HubCommunicator.GetCratesByDirection(ActivityId, direction, CurrentUserId);
        }

        protected async Task<ActivityTemplateDTO> GetActivityTemplate(Guid activityTemplateId)
        {
            var allActivityTemplates = _activityTemplateCache ?? (_activityTemplateCache = await HubCommunicator.GetActivityTemplates(CurrentUserId));

            var foundActivity = allActivityTemplates.FirstOrDefault(a => a.Id == activityTemplateId);


            if (foundActivity == null)
            {
                throw new Exception($"ActivityTemplate was not found. Id: {activityTemplateId}");
            }

            return foundActivity;
        }

        protected async Task<ActivityTemplateDTO> GetActivityTemplate(string terminalName, string activityTemplateName, string activityTemplateVersion = "1", string terminalVersion = "1")
        {
            var allActivityTemplates = _activityTemplateCache ?? (_activityTemplateCache = await HubCommunicator.GetActivityTemplates(CurrentUserId));

            var foundActivity = allActivityTemplates.FirstOrDefault(a =>
                        a.Terminal.Name == terminalName && a.Terminal.Version == terminalVersion &&
                        a.Name == activityTemplateName && a.Version == activityTemplateVersion);


            if (foundActivity == null)
            {
                throw new Exception($"ActivityTemplate was not found. TerminalName: {terminalName}\nTerminalVersion: {terminalVersion}\nActivitiyTemplateName: {activityTemplateName}\nActivityTemplateVersion: {activityTemplateVersion}");
            }

            return foundActivity;
        }

        public SolutionPageDTO GetDefaultDocumentation(string solutionName, double solutionVersion, string terminalName, string body)
        {
            var curSolutionPage = new SolutionPageDTO
            {
                Name = solutionName,
                Version = solutionVersion,
                Terminal = terminalName,
                Body = body
            };

            return curSolutionPage;
        }

        public ActivityResponseDTO GenerateErrorResponse(string errorMessage)
        {
            return new ActivityResponseDTO
            {
                Body = errorMessage,
                Type = ActivityResponse.ShowDocumentation.ToString()
            };
        }

        public async Task Run(ActivityContext activityContext, ContainerExecutionContext containerExecutionContext)
        {
            InitializeActivity(activityContext, containerExecutionContext);
            await Run();
        }

        public virtual Task RunChildActivities()
        {
            return Task.FromResult(0);
        }

        public abstract Task Run();
        public abstract Task Initialize();
        public abstract Task FollowUp();

        public virtual Task Activate()
        {
            return Task.FromResult(0);
        }

        public virtual Task Deactivate()
        {
            return Task.FromResult(0);
        }
        public async Task Configure(ActivityContext activityContext)
        {
            InitializeActivity(activityContext);
            try
            {
                if (AuthorizeIfNecessary())
                {
                    return;
                }

                var configurationType = GetConfigurationRequestType();
                switch (configurationType)
                {
                    case ConfigurationRequestType.Initial:
                        await Initialize();
                        break;

                    case ConfigurationRequestType.Followup:
                        await FollowUp();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException($"Unsupported configuration type: {configurationType}");
                }
            }
            catch (Exception ex)
            {
                if (IsTokenInvalidation(ex))
                {
                    AddAuthenticationCrate(true);
                }

                throw;
            }
        }

        protected virtual Task<bool> Validate()
        {
            return Task.FromResult(true);
        }

        public async Task Activate(ActivityContext activityContext)
        {
            InitializeActivity(activityContext);
            if (AuthorizeIfNecessary())
            {
                return;
            }

            await Activate();
        }

        public async Task Deactivate(ActivityContext activityContext)
        {
<<<<<<< HEAD
            InitializeActivity(activityContext);
            await Deactivate();
        }

=======
            return new ActivityResponseDTO
            {
                Body = documentation,
                Type = ActivityResponse.ShowDocumentation.ToString()
            };
        }
        public ActivityResponseDTO GenerateErrorResponse(string errorMessage)
        {
            return new ActivityResponseDTO
            {
                Body = errorMessage,
                Type = ActivityResponse.ShowDocumentation.ToString()
            };
        }

        public SolutionPageDTO GetDefaultDocumentation(string solutionName, double solutionVersion, string terminalName, string body)
        {
            var curSolutionPage = new SolutionPageDTO
            {
                Name = solutionName,
                Version = solutionVersion,
                Terminal = terminalName,
                Body = body
            };

            return curSolutionPage;
        }

        /*******************************************************************************************/
        // Working with payload values
        /*******************************************************************************************/

        /// <summary>
        /// Extracts crate with specified label and ManifestType = Standard Design Time,
        /// then extracts field with specified fieldKey.
        /// </summary>
        protected string ExtractPayloadFieldValue(ICrateStorage crateStorage, string fieldKey, ActivityDO curActivity)
        {
            var fieldValues = crateStorage.CratesOfType<StandardPayloadDataCM>().SelectMany(x => x.Content.GetValues(fieldKey))
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();

            if (fieldValues.Length > 0)
                return fieldValues[0];

            var baseEvent = new BaseTerminalEvent();
            var exceptionMessage = string.Format("No field found with specified key: {0}.", fieldKey);
            //This is required for proper logging of the Incidents
            var fr8UserId = curActivity.AuthorizationToken.UserID;
            baseEvent.SendTerminalErrorIncident(ActivityName, exceptionMessage, ActivityName, fr8UserId);

            throw new ApplicationException(exceptionMessage);
        }

        /*******************************************************************************************/
        // Working with upstream
        /*******************************************************************************************/

        //wrapper for support test method
        public virtual async Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(ActivityDO activityDO, CrateDirection direction)
        {
            return await HubCommunicator.GetCratesByDirection<TManifest>(activityDO, direction, CurrentFr8UserId);
            // return await Activity.GetCratesByDirection<TManifest>(activityId, direction);
        }

        public Task<IncomingCratesDTO> GetAvailableData(ActivityDO activity, CrateDirection direction = CrateDirection.Upstream, AvailabilityType availability = AvailabilityType.RunTime)
        {
            return HubCommunicator.GetAvailableData(activity, direction, availability, CurrentFr8UserId);
        }

        //wrapper for support test method
        public virtual async Task<List<Crate>> GetCratesByDirection(ActivityDO activityDO, CrateDirection direction)
        {
            return await HubCommunicator.GetCratesByDirection(activityDO, direction, CurrentFr8UserId);
        }

        public virtual async Task<FieldDescriptionsCM> GetDesignTimeFields(ActivityDO activityDO, CrateDirection direction, AvailabilityType availability = AvailabilityType.NotSet)
        {
            var mergedFields = await HubCommunicator.GetDesignTimeFieldsByDirection(activityDO, direction, availability, CurrentFr8UserId);
            return mergedFields;
        }

        public virtual IEnumerable<FieldDTO> GetRequiredFields(ActivityDO curActivityDO, string crateLabel)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var requiredFields = crateStorage
                                        .CrateContentsOfType<FieldDescriptionsCM>(c => c.Label.Equals(crateLabel))
                                        .SelectMany(f => f.Fields.Where(s => s.IsRequired));

                return requiredFields;
            }
        }

        public virtual async Task<List<CrateManifestType>> BuildUpstreamManifestList(ActivityDO activityDO)
        {
            var upstreamCrates = await this.GetCratesByDirection<Fr8Data.Manifests.Manifest>(activityDO, CrateDirection.Upstream);
            return upstreamCrates.Where(x => !ExcludedManifestTypes.Contains(x.ManifestType)).Select(f => f.ManifestType).Distinct().ToList();
        }

        public virtual async Task<List<String>> BuildUpstreamCrateLabelList(ActivityDO activityDO)
        {
            var curCrates = await this.GetCratesByDirection<Fr8Data.Manifests.Manifest>(activityDO, CrateDirection.Upstream);
            return curCrates.Where(x => !ExcludedManifestTypes.Contains(x.ManifestType)).Select(f => f.Label).Distinct().ToList();
        }

        public virtual async Task<Crate<FieldDescriptionsCM>> GetUpstreamManifestListCrate(ActivityDO activityDO)
        {
            var manifestList = (await BuildUpstreamManifestList(activityDO));
            var fields = manifestList.Select(f => new FieldDTO(f.Type, f.Id.ToString())).ToArray();

            return CrateManager.CreateDesignTimeFieldsCrate("AvailableUpstreamManifests", fields);
        }

        public virtual async Task<Crate<FieldDescriptionsCM>> GetUpstreamCrateLabelListCrate(ActivityDO activityDO)
        {
            var labelList = (await BuildUpstreamCrateLabelList(activityDO));
            var fields = labelList.Select(f => new FieldDTO(f, f)).ToArray();

            return CrateManager.CreateDesignTimeFieldsCrate("AvailableUpstreamLabels", fields);
        }


        protected virtual async Task<List<Crate<StandardFileDescriptionCM>>> GetUpstreamFileHandleCrates(ActivityDO activityDO)
        {
            return await HubCommunicator.GetCratesByDirection<StandardFileDescriptionCM>(activityDO, CrateDirection.Upstream, CurrentFr8UserId);
        }

        protected async Task<Crate<FieldDescriptionsCM>> MergeUpstreamFields(ActivityDO activityDO, string label)
        {
            var curUpstreamFields = (await GetDesignTimeFields(activityDO, CrateDirection.Upstream)).Fields.ToArray();
            var upstreamFieldsCrate = CrateManager.CreateDesignTimeFieldsCrate(label, curUpstreamFields);

            return upstreamFieldsCrate;
        }

        /// <summary>
        /// Creates a crate with available design-time fields.
        /// </summary>
        /// <param name="activityDO">ActionDO.</param>
        /// <returns></returns>
        protected async Task<Crate> CreateAvailableFieldsCrate(ActivityDO activityDO,
            string crateLabel = "Upstream Terminal-Provided Fields",
            AvailabilityType availabilityTypeUpstream = AvailabilityType.RunTime,
            AvailabilityType availabilityTypeFieldsCrate = AvailabilityType.Configuration)
        {
            var curUpstreamFields = await HubCommunicator.GetDesignTimeFieldsByDirection(activityDO, CrateDirection.Upstream, availabilityTypeUpstream, CurrentFr8UserId) ?? new FieldDescriptionsCM();

            var availableFieldsCrate = CrateManager.CreateDesignTimeFieldsCrate(
                    crateLabel,
                    curUpstreamFields.Fields,
                    availabilityTypeFieldsCrate);

            return availableFieldsCrate;
        }

        // create crate with list of fields available upstream
        protected async Task<Crate<FieldDescriptionsCM>> CreateDesignTimeFieldsCrate(ActivityDO curActivityDO, string label)
        {
            List<Crate<FieldDescriptionsCM>> crates = null;

            try
            {
                //throws exception from test classes when it cannot call webservice
                crates = await GetCratesByDirection<FieldDescriptionsCM>(curActivityDO, CrateDirection.Upstream);
            }
            catch { }

            if (crates != null)
            {
                var upstreamFields = crates.SelectMany(x => x.Content.Fields).Where(a => a.Availability != AvailabilityType.Configuration).ToArray();

                return CrateManager.CreateDesignTimeFieldsCrate(label, upstreamFields);
            }

            return null;
        }

        /// <summary>
        /// Extract upstream data based on Upstream Crate Chooser Control's selected manifest and label 
        /// </summary>
        /// <param name="upstreamCrateChooserName">Upstream Crate Chooser Control's name</param>
        /// <param name="curActivityDO"></param>
        /// <returns>StandardTableDataCM. StandardTableDataCM.Table.Count = 0 if its empty</returns>
        protected async Task<StandardTableDataCM> ExtractDataFromUpstreamCrates(string upstreamCrateChooserName, ActivityDO curActivityDO)
        {
            var controls = GetConfigurationControls(curActivityDO);
            var crateChooser = controls.FindByName<UpstreamCrateChooser>(upstreamCrateChooserName);

            //Get selected manifest and label from crate chooser
            var manifestType = crateChooser.SelectedCrates.Where(w => !String.IsNullOrEmpty(w.ManifestType.Value)).Select(c => c.ManifestType.Value);
            var labelControl = crateChooser.SelectedCrates.Where(w => !String.IsNullOrEmpty(w.Label.Value)).Select(c => c.Label.Value);

            //filter by manifest or label based on selected crate chooser control
            Func<Crate, bool> whereClause = null;
            if (manifestType.Any() && labelControl.Any())
                whereClause = (crate => manifestType.Contains(crate.ManifestType.Id.ToString()) && labelControl.Contains(crate.Label));
            else if (manifestType.Any())
                whereClause = (crate => manifestType.Contains(crate.ManifestType.Id.ToString()));
            else if (labelControl.Any())
                whereClause = (crate => labelControl.Contains(crate.Label));


            //get upstream controls to extract the data based on the selected manifest or label
            var getUpstreamCrates = (await GetCratesByDirection(curActivityDO, CrateDirection.Upstream)).ToArray();
            var selectedUpcrateControls = getUpstreamCrates.Where(whereClause).Select(s => s);//filter upstream controls

            StandardTableDataCM resultTable = new StandardTableDataCM();
            foreach (var selectedUpcrateControl in selectedUpcrateControls)
            {
                if (selectedUpcrateControl != null)
                {
                    //for standard table data CM return directly the result - the rest of CM will be converted to TableDataCM
                    if (selectedUpcrateControl.IsOfType<StandardTableDataCM>())
                    {
                        var contentTableCM = selectedUpcrateControl.Get<StandardTableDataCM>();
                        resultTable.Table.AddRange(contentTableCM.Table);//add rows into results StandardTableDataCM
                    }
                    else
                        throw new NotSupportedException("Manifest type is not yet implemented in extracting the data.");
                }
            }

            return resultTable;
        }

        /*******************************************************************************************/
        // Working with controls
        /*******************************************************************************************/

        /// <summary>
        /// This is a generic function for creating a CrateChooser which is suitable for most use-cases
        /// </summary>
        /// <param name="curActivityDO"></param>
        /// <param name="label"></param>
        /// <param name="name"></param>
        /// <param name="singleManifest"></param>
        /// <returns></returns>
        protected async Task<CrateChooser> GenerateCrateChooser(
            ActivityDO curActivityDO,
            string name,
            string label,
            bool singleManifest,
            bool requestUpstream = false,
            bool requestConfig = false)
        {
            var crateDescriptions = await HubCommunicator.GetAvailableData(curActivityDO, CrateDirection.Upstream, AvailabilityType.Always, CurrentFr8UserId);

            var control = new CrateChooser
            {
                Label = label,
                Name = name,
                CrateDescriptions = crateDescriptions.AvailableCrates,
                SingleManifestOnly = singleManifest,
                RequestUpstream = requestUpstream
            };

            if (requestConfig)
            {
                control.Events.Add(new ControlEvent("onChange", "requestConfig"));
            }

            return control;
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
        protected void AddTextSourceControl(
            ICrateStorage storage,
            string label,
            string controlName,
            string upstreamSourceLabel,
            string filterByTag = "",
            bool addRequestConfigEvent = false,
            bool required = false,
            bool requestUpstream = false)
        {
            var textSourceControl = CreateSpecificOrUpstreamValueChooser(
                label,
                controlName,
                upstreamSourceLabel,
                filterByTag,
                addRequestConfigEvent,
                requestUpstream
            );
            textSourceControl.Required = required;

            AddControl(storage, textSourceControl);
        }

        /// <summary>
        /// Creates RadioButtonGroup to enter specific value or choose value from upstream crate.
        /// </summary>
        protected TextSource CreateSpecificOrUpstreamValueChooser(
            string label,
            string controlName,
            string upstreamSourceLabel = "",
            string filterByTag = "",
            bool addRequestConfigEvent = false,
            bool requestUpstream = false)
        {
            var control = new TextSource(label, upstreamSourceLabel, controlName)
            {
                Source = new FieldSourceDTO
                {
                    Label = upstreamSourceLabel,
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields,
                    FilterByTag = filterByTag,
                    RequestUpstream = requestUpstream
                }
            };
            if (addRequestConfigEvent)
            {
                control.Events.Add(new ControlEvent("onChange", "requestConfig"));
            }

            return control;
        }

        protected UpstreamCrateChooser CreateUpstreamCrateChooser(string name, string label, bool isMultiSelection = true)
        {

            var manifestDdlb = new DropDownList { Name = name + "_mnfst_dropdown_0", Source = new FieldSourceDTO(CrateManifestTypes.StandardDesignTimeFields, "AvailableUpstreamManifests") };
            var labelDdlb = new DropDownList { Name = name + "_lbl_dropdown_0", Source = new FieldSourceDTO(CrateManifestTypes.StandardDesignTimeFields, "AvailableUpstreamLabels") };

            var ctrl = new UpstreamCrateChooser
            {
                Name = name,
                Label = label,
                SelectedCrates = new List<CrateDetails> { new CrateDetails { Label = labelDdlb, ManifestType = manifestDdlb } },
                MultiSelection = isMultiSelection
            };

            return ctrl;
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

        /*******************************************************************************************/
        // Deprecated methods
        // Can be refactored if necessary
        /*******************************************************************************************/

        //looks for the Configuration Controls Crate and Extracts the ManifestSchema
        protected StandardConfigurationControlsCM GetControlsManifest(ActivityDO curActivity)
        {
            var control = CrateManager.GetStorage(curActivity.CrateStorage).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (control == null)
            {
                throw new ApplicationException($"No crate found with Label == \"Configuration_Controls\" and ManifestType == \"{CrateManifestTypes.StandardConfigurationControls}\"");
            }

            return control;
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

        protected T GetControl<T>(StandardConfigurationControlsCM controls, string name, string controlType = null) where T : ControlDefinitionDTO
        {
            return (T)GetControl(controls, name, controlType);
        }

        protected ControlDefinitionDTO GetControl(ActivityDO curActivityDO, string name, string controlType = null)
        {
            var controls = GetConfigurationControls(curActivityDO);
            return GetControl(controls, name, controlType);
        }

        // do not use after EnhancedTerminalActivity is introduced
        protected ICrateStorage AssembleCrateStorage(params Crate[] curCrates)
        {
            return new CrateStorage(curCrates);
        }

        // do not use after EnhancedTerminalActivity is introduced
        protected Crate PackControls(StandardConfigurationControlsCM page)
        {
            return PackControlsCrate(page.Controls.ToArray());
        }

        // do not use after EnhancedTerminalActivity is introduced
        protected Crate<StandardConfigurationControlsCM> PackControlsCrate(params ControlDefinitionDTO[] controlsList)
        {
            return Crate<StandardConfigurationControlsCM>.FromContent(ConfigurationControlsLabel, new StandardConfigurationControlsCM(controlsList), AvailabilityType.Configuration);
        }

        // do not use after EnhancedTerminalActivity is introduced
        protected string ExtractControlFieldValue(ActivityDO curActivityDO, string fieldName)
        {
            var storage = CrateManager.GetStorage(curActivityDO);
            var controlsCrateMS = storage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            var field = controlsCrateMS?.Controls.FirstOrDefault(x => x.Name == fieldName);

            return field?.Value;
        }


        protected void AddLabelControl(ICrateStorage storage, string name, string label, string text)
        {
            AddControl(
                storage,
                GenerateTextBlock(label, text, "well well-lg", name)
            );
        }

        protected void AddControl(ICrateStorage storage, ControlDefinitionDTO control)
        {
            var controlsCrate = EnsureControlsCrate(storage);

            if (controlsCrate.Content == null) { return; }

            controlsCrate.Content.Controls.Add(control);
        }

        protected void InsertControlAfter(ICrateStorage storage, ControlDefinitionDTO control, string afterControlName)
        {
            var controlsCrate = EnsureControlsCrate(storage);

            if (controlsCrate.Content == null) { return; }

            for (var i = 0; i < controlsCrate.Content.Controls.Count; ++i)
            {
                if (controlsCrate.Content.Controls[i].Name == afterControlName)
                {
                    if (i == controlsCrate.Content.Controls.Count - 1)
                    {
                        controlsCrate.Content.Controls.Add(control);
                    }
                    else
                    {
                        controlsCrate.Content.Controls.Insert(i + 1, control);
                    }

                    break;
                }
            }
        }

        protected ControlDefinitionDTO FindControl(ICrateStorage storage, string name)
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
            using (var crateStorage = CrateManager.GetUpdatableStorage(activity))
            {
                var controls = crateStorage
                    .CrateContentsOfType<StandardConfigurationControlsCM>()
                    .FirstOrDefault();

                if (controls != null)
                {
                    var control = TraverseNestedControls(controls.Controls, controlFullName);

                    if (control != null)
                    {
                        switch (control.Type)
                        {
                            case "TextBlock":
                            case "TextBox":
                            case "BuildMessageAppender":
                            case ControlTypes.TextArea:
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
                                throw new NotSupportedException($"Unsupported control type {control.Type}");
                        }
                    }
                }
            }
        }

        private ControlDefinitionDTO TraverseNestedControls(List<ControlDefinitionDTO> controls, string childControl)
        {
            ControlDefinitionDTO controlDefinitionDTO = null;
            var controlNames = childControl.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (controlNames.Length > 0 && controls.Count > 0)
            {
                var control = controls.FirstOrDefault(a => a.Name == controlNames[0]);

                if (control != null)
                {
                    if (controlNames.Count() == 1)
                    {
                        controlDefinitionDTO = control;
                    }
                    else
                    {
                        List<ControlDefinitionDTO> nestedControls = null;

                        if (control.Type == "RadioButtonGroup")
                        {
                            var radio = (control as RadioButtonGroup).Radios.FirstOrDefault(a => a.Name == controlNames[1]);
                            if (radio != null)
                            {
                                radio.Selected = true;
                                nestedControls = radio.Controls.ToList();

                                controlDefinitionDTO = TraverseNestedControls(nestedControls, string.Join(".", controlNames.Skip(2)));
                            }
                        }
                        //TODO: Add support for future controls with nested child controls
                        else
                            throw new NotImplementedException("Can't search for controls inside of " + control.Type);
                    }
                }
            }

            return controlDefinitionDTO;
        }

        protected void RemoveControl(ICrateStorage storage, string name)
        {
            var controlsCrate = storage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (controlsCrate == null) { return; }


            var control = controlsCrate.Controls.FirstOrDefault(x => x.Name == name);

            if (control != null)
            {
                controlsCrate.Controls.Remove(control);
            }
        }

        protected Crate<StandardConfigurationControlsCM> EnsureControlsCrate(ICrateStorage storage)
        {
            var controlsCrate = storage.CratesOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (controlsCrate == null)
            {
                controlsCrate = CrateManager.CreateStandardConfigurationControlsCrate(ConfigurationControlsLabel);
                storage.Add(controlsCrate);
            }

            return controlsCrate;
        }

        public string ParseConditionToText(List<FilterConditionDTO> filterData)
        {
            var parsedConditions = new List<string>();
            
            filterData?.ForEach(condition =>
            {
                string parsedCondition = condition.Field;

                switch (condition.Operator)
                {
                    case "eq":
                        parsedCondition += " = ";
                        break;
                    case "neq":
                        parsedCondition += " != ";
                        break;
                    case "gt":
                        parsedCondition += " > ";
                        break;
                    case "gte":
                        parsedCondition += " >= ";
                        break;
                    case "lt":
                        parsedCondition += " < ";
                        break;
                    case "lte":
                        parsedCondition += " <= ";
                        break;
                    default:
                        throw new NotSupportedException(string.Format("Not supported operator: {0}", condition.Operator));
                }

                parsedCondition += $"'{condition.Value}'";
                parsedConditions.Add(parsedCondition);
            });

            var finalCondition = string.Join(" AND ", parsedConditions);

            return finalCondition;
        }

        // do not use after EnhancedTerminalActivity is introduced
        protected StandardConfigurationControlsCM GetConfigurationControls(ActivityDO curActivityDO)
        {
            var storage = CrateManager.GetStorage(curActivityDO);
            return GetConfigurationControls(storage);
        }

        // do not use after EnhancedTerminalActivity is introduced
        protected StandardConfigurationControlsCM GetConfigurationControls(ICrateStorage storage)
        {
            var controlsCrate = storage.CrateContentsOfType<StandardConfigurationControlsCM>(c => c.Label == ConfigurationControlsLabel).FirstOrDefault();
            return controlsCrate;
        }

        // do not use after EnhancedTerminalActivity is introduced
        /// <summary>
        /// Method to be used with Loop Action
        /// Is a helper method to decouple some of the GetCurrentElement Functionality
        /// </summary>
        /// <param name="operationalCrate">Crate of the OperationalStateCM</param>
        /// <returns>Index or pointer of the current IEnumerable Object</returns>
        protected int GetLoopIndex(OperationalStateCM operationalState)
        {
            var loopState = operationalState.CallStack.FirstOrDefault(x => x.LocalData?.Type == "Loop");

            if (loopState != null) //this is a loop related data request
            {
                return loopState.LocalData.ReadAs<OperationalStateCM.LoopStatus>().Index;
            }

            throw new NullReferenceException("No Loop was found inside the provided OperationalStateCM crate");
        }

        // do not use after EnhancedTerminalActivity is introduced
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

        protected OperationalStateCM GetOperationalStateCrate(ICrateStorage storage)
        {
            var operationalState = storage.CrateContentsOfType<OperationalStateCM>().SingleOrDefault();
            if (operationalState == null)
                throw new Exception("No Operational State Crate found.");
            return operationalState;
        }
>>>>>>> refs/remotes/origin/dev
    }
}