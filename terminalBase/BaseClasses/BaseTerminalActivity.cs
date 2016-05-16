using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StructureMap;
using TerminalBase.Infrastructure;
using Fr8Data.Constants;
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
            HubCommunicator.Configure(MyTemplate.Terminal.Name);
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

        private ValidationManager _validationManager;
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
        protected ValidationManager ValidationManager => _validationManager ?? (_validationManager = new ValidationManager());
        protected Guid ActivityId => ActivityContext.ActivityPayload.Id;
        protected ActivityPayload ActivityPayload => ActivityContext.ActivityPayload;
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

        protected virtual ConfigurationRequestType GetConfigurationRequestType()
        {
            return Storage.Count == 0 ? ConfigurationRequestType.Initial : ConfigurationRequestType.Followup;
        }

        protected StandardConfigurationControlsCM GetConfigurationControls()
        {
            return Storage.CrateContentsOfType<StandardConfigurationControlsCM>(c => c.Label == ConfigurationControlsLabel).FirstOrDefault();
        }

        protected T GetControl<T>(string name, string controlType = null) where T : ControlDefinitionDTO
        {
            Func<ControlDefinitionDTO, bool> predicate = x => x.Name == name;
            if (controlType != null)
            {
                predicate = x => x.Type == controlType && x.Name == name;
            }

            return (T) ConfigurationControls.Controls.FirstOrDefault(predicate);
        }


        private bool AuthorizeIfNecessary()
        {
            if (IsAuthenticationRequired)
            {
                return CheckAuthentication();
            }

            return false;
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

            ConfigurationControls.Controls.Add(control);
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

        protected async Task ValidateAndConfigure()
        {
            Storage.Remove<ValidationResultsCM>();
            ValidationManager.Reset();
            await Validate();
            if (ValidationManager.HasErrors)
            {
                Storage.Add(Crate.FromContent("Validation Results", ValidationManager.GetResults()));
                return;
            }
            await FollowUp();
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
                        await ValidateAndConfigure();
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
            InitializeActivity(activityContext);
            await Deactivate();
        }

        protected Crate<StandardConfigurationControlsCM> PackControlsCrate(params ControlDefinitionDTO[] controlsList)
        {
            return Crate<StandardConfigurationControlsCM>.FromContent(ConfigurationControlsLabel, new StandardConfigurationControlsCM(controlsList), AvailabilityType.Configuration);
        }

        protected Crate PackControls(StandardConfigurationControlsCM page)
        {
            return PackControlsCrate(page.Controls.ToArray());
        }

        protected Crate<StandardConfigurationControlsCM> EnsureControlsCrate()
        {
            var controlsCrate = Storage.CratesOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (controlsCrate == null)
            {
                controlsCrate = CrateManager.CreateStandardConfigurationControlsCrate(ConfigurationControlsLabel);
                Storage.Add(controlsCrate);
            }

            return controlsCrate;
        }

    }
}