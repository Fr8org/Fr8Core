using System;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.DataTransferObjects.Helpers;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.Errors;
using Fr8.TerminalBase.Helpers;
using Fr8.TerminalBase.Infrastructure;
using Fr8.TerminalBase.Infrastructure.States;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using Fr8.TerminalBase.Services;

namespace Fr8.TerminalBase.BaseClasses
{
    // Almost minimal activity implementation
    public abstract class TerminalActivityBase : IActivity
    {
        /**********************************************************************************/
        // Declarations
        /**********************************************************************************/

        public const string ConfigurationControlsLabel = "Configuration_Controls";
        public const string ValidationCrateLabel = "Validation Results";

        /**********************************************************************************/

        private OperationalStateCM _operationalState;
        private ActivityContext _activityContext;
        private ContainerExecutionContext _containerExecutionContext;
        private ControlHelper _controlHelper;
        
        /**********************************************************************************/

        protected ICrateStorage Payload
        {
            get
            {
                CheckRunTime("Payload storage is not available at the design time");
                return _containerExecutionContext.PayloadStorage;
            }
        }

        protected OperationalStateCM OperationalState
        {
            get
            {
                CheckRunTime("Operations state is not available at the design time");
                return _operationalState;
            }
        }

        protected ContainerExecutionContext ExecutionContext
        {
            get
            {
                CheckRunTime("Execution context is not available at the design time");
                return _containerExecutionContext;
            }
        }

        protected ICrateStorage Storage => _activityContext.ActivityPayload.CrateStorage;
        protected ActivityPayload ActivityPayload => _activityContext.ActivityPayload;
        protected AuthorizationToken AuthorizationToken => _activityContext.AuthorizationToken;
        protected IHubCommunicator HubCommunicator => _activityContext.HubCommunicator;
        protected ActivityContext ActivityContext => _activityContext;
        protected bool IsRuntime => _containerExecutionContext != null;
        protected AuthenticationMode AuthenticationMode { get; set; } = AuthenticationMode.InternalMode;
        protected bool DisableValidationOnFollowup { get; set; }
        protected ICrateManager CrateManager { get; private set;}
        protected int LoopIndex => GetLoopIndex();
        protected ControlHelper ControlHelper => _controlHelper ?? (_controlHelper = new ControlHelper(ActivityContext));
        protected ValidationManager ValidationManager { get; private set; }
        protected Guid ActivityId => ActivityContext.ActivityPayload.Id;
        protected string CurrentUserId => ActivityContext.UserId;

        protected abstract ActivityTemplateDTO MyTemplate { get; }

        public CrateSignaller CrateSignaller { get; private set; }

        /**********************************************************************************/
        // Functions
        /**********************************************************************************/

        protected TerminalActivityBase(ICrateManager crateManager)
        {
            CrateManager = crateManager;
        }

        /**********************************************************************************/

        private void InitializeInternalState(ActivityContext activityContext, ContainerExecutionContext containerExecutionContext)
        {
            _activityContext = activityContext;
            _containerExecutionContext = containerExecutionContext;

            if (_containerExecutionContext != null)
            {
                _operationalState = Payload.CrateContentsOfType<OperationalStateCM>().FirstOrDefault();

                if (_operationalState == null)
                {
                    throw new InvalidOperationException("Operational state crate is not found");
                }
            }

            InitializeInternalState();
        }

        /**********************************************************************************/

        protected virtual Task<bool> CheckAuthentication()
        {
            return Task.FromResult(!MyTemplate.NeedsAuthentication || !string.IsNullOrEmpty(AuthorizationToken?.Token));
        }

        /**********************************************************************************/

        protected void AddAuthenticationCrate(bool revocation)
        {
            Storage.Remove<StandardAuthenticationCM>();
            Storage.Add(Crate.FromContent("RequiresAuthentication", new StandardAuthenticationCM
            {
                Mode = AuthenticationMode,
                Revocation = revocation
            }));
        }

        /**********************************************************************************/

        public async Task Configure(ActivityContext activityContext)
        {
            InitializeInternalState(activityContext, null);
            var configurationType = GetConfigurationRequestType();
            bool afterConfigureFails = false;

            try
            {
                if (!await CheckAuthentication())
                {
                    AddAuthenticationCrate(false);
                    return;
                }
                
                if (!await BeforeConfigure(configurationType))
                {
                    return;
                }

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
                try
                {
                    await AfterConfigure(configurationType, null);
                }
                catch
                {
                    afterConfigureFails = true;
                    throw;
                }
            }
            catch (AuthorizationTokenExpiredOrInvalidException)
            {
                AddAuthenticationCrate(true);
            }
            catch (Exception ex)
            {
                if (IsInvalidTokenException(ex))
                {
                    AddAuthenticationCrate(true);
                }
                else if (!afterConfigureFails)
                {
                    await AfterConfigure(configurationType, ex);
                }
            }
        }

        /**********************************************************************************/

        private void CheckRunTime(string message = "Not available at the design time")
        {
            if (!IsRuntime)
            {
                throw new InvalidOperationException(message);
            }
        }

        /**********************************************************************************/

        public async Task Activate(ActivityContext activityContext)
        {
            InitializeInternalState(activityContext, null);

            if (!await BeforeActivate())
            {
                return;
            }

            bool afterActivateFails = false;

            try
            {
                await Activate();

                try
                {
                    await AfterActivate(null);
                }
                catch
                {
                    afterActivateFails = true;
                    throw;
                }
            }
            catch (Exception ex)
            {
                if (!afterActivateFails)
                {
                    await AfterActivate(ex);
                }
            }
        }

        /**********************************************************************************/

        public async Task Deactivate(ActivityContext activityContext)
        {
            InitializeInternalState(activityContext, null);

            if (!await BeforeDeactivate())
            {
                return;
            }

            bool afterDeactivateFails = false;

            try
            {
                await Deactivate();

                try
                {
                    await AfterDeactivate(null);
                }
                catch
                {
                    afterDeactivateFails = true;
                    throw;
                }
            }
            catch (Exception ex)
            {
                if (!afterDeactivateFails)
                {
                    await AfterDeactivate(ex);
                }
            }
        }

        /**********************************************************************************/

        public Task Run(ActivityContext activityContext, ContainerExecutionContext containerExecutionContext)
        {
            InitializeInternalState(activityContext, containerExecutionContext);

            return Run(Run);
        }

        /**********************************************************************************/

        public Task RunChildActivities(ActivityContext activityContext, ContainerExecutionContext containerExecutionContext)
        {
            InitializeInternalState(activityContext, containerExecutionContext);

            return Run(RunChildActivities);
        }

        /**********************************************************************************/

        private async Task Run(Func<Task> runMode)
        {
            if (!await CheckAuthentication())
            {
                if (string.IsNullOrWhiteSpace(AuthorizationToken?.Token))
                {
                    ErrorInvalidToken("No AuthToken provided.");
                }
                else
                {
                    ErrorInvalidToken("Authorization token is invalid");
                }
                
                return;
            }

            bool afterRunFails = false;

            try
            {
                OperationalState.CurrentActivityResponse = null;

                if (await BeforeRun())
                {
                    await runMode();

                    try
                    {
                        await AfterRun(null);
                    }
                    catch
                    {
                        afterRunFails = true;
                        throw;
                    }
                }

                if (OperationalState.CurrentActivityResponse == null)
                {
                    Success();
                }
            }
            catch (AuthorizationTokenExpiredOrInvalidException ex)
            {
                if (!afterRunFails)
                {
                    await AfterRun(ex);
                }

                ErrorInvalidToken(ex.Message);
            }
            catch (ActivityExecutionException ex)
            {
                if (!afterRunFails)
                {
                    await AfterRun(ex);
                }

                Error(ex.Message, ex.ErrorCode);
            }
            catch (AggregateException ex)
            {
                if (!afterRunFails)
                {
                    await AfterRun(ex);
                }

                foreach (var innerException in ex.InnerExceptions)
                {
                    if (IsInvalidTokenException(innerException))
                    {
                        ErrorInvalidToken(ex.Message);
                        return;
                    }
                }

                Error(ex.Flatten().Message);
            }
            catch (Exception ex)
            {
                if (!afterRunFails)
                {
                    await AfterRun(ex);
                }

                if (IsInvalidTokenException(ex))
                {
                    ErrorInvalidToken(ex.Message);
                }
                else
                {
                    Error(ex.Message);
                }
            }
        }

        /**********************************************************************************/

        protected virtual ConfigurationRequestType GetConfigurationRequestType()
        {
            return Storage.Count == 0 ? ConfigurationRequestType.Initial : ConfigurationRequestType.Followup;
        }

        /**********************************************************************************/

        public abstract Task Initialize();
        public abstract Task FollowUp();
        public abstract Task Run();

        /**********************************************************************************/

        protected virtual void InitializeInternalState()
        {
            var terminalAuthType = MyTemplate.Terminal.AuthenticationType;

            switch (terminalAuthType)
            {
                case AuthenticationType.Internal:
                    AuthenticationMode = AuthenticationMode.InternalMode;
                    break;

                case AuthenticationType.External:
                    AuthenticationMode = AuthenticationMode.ExternalMode;
                    break;

                case AuthenticationType.InternalWithDomain:
                    AuthenticationMode = AuthenticationMode.InternalModeWithDomain;
                    break;

                case AuthenticationType.None:
                    AuthenticationMode = AuthenticationMode.ExternalMode;
                    break;

                default:
                    throw new Exception("Unknown authentication type: " + terminalAuthType);
            }

            CrateSignaller = new CrateSignaller(Storage, MyTemplate.Name, ActivityId);
        }

        /**********************************************************************************/

        public virtual Task RunChildActivities()
        {
            return Task.FromResult(0);
        }

        /**********************************************************************************/

        public virtual Task Activate()
        {
            return Task.FromResult(0);
        }

        /**********************************************************************************/

        public virtual Task Deactivate()
        {
            return Task.FromResult(0);
        }

        /**********************************************************************************/

        protected virtual Task AfterRun(Exception ex)
        {
            return Task.FromResult(0);
        }

        /**********************************************************************************/

        protected virtual async Task<bool> BeforeRun()
        {
            ValidationManager = CreateValidationManager();

            await Validate();

            if (ValidationManager.HasErrors)
            {
                RaiseError("Activity was incorrectly configured. " + ValidationManager.ValidationResults);
                return false;
            }

            return true;
        }

        /**********************************************************************************/

        protected virtual Task AfterDeactivate(Exception ex)
        {
            return Task.FromResult(0);
        }

        /**********************************************************************************/

        protected virtual Task<bool> BeforeDeactivate()
        {
            return Task.FromResult(true);
        }

        /**********************************************************************************/

        protected virtual Task AfterActivate(Exception ex)
        {
            return Task.FromResult(0);
        }

        /**********************************************************************************/

        protected virtual async Task<bool> BeforeActivate()
        {
            Storage.Remove<ValidationResultsCM>();

            ValidationManager = CreateValidationManager();

            await Validate();

            if (ValidationManager.HasErrors)
            {
                Storage.Add(Crate.FromContent(ValidationCrateLabel, ValidationManager.ValidationResults));
                return false;
            }

            return true;
        }

        /**********************************************************************************/

        protected virtual Task AfterConfigure(ConfigurationRequestType configurationRequestType, Exception ex)
        {
            return Task.FromResult(0);
        }

        /**********************************************************************************/

        protected virtual async Task<bool> BeforeConfigure(ConfigurationRequestType configurationRequestType)
        {
            if (configurationRequestType == ConfigurationRequestType.Initial)
            {
                return true;
            }

            Storage.Remove<ValidationResultsCM>();

            ValidationManager = CreateValidationManager();

            ValidationManager.Reset();

            if (!DisableValidationOnFollowup)
            {
                await Validate();
            }

            if (ValidationManager.HasErrors)
            {
                Storage.Add(Crate.FromContent(ValidationCrateLabel, ValidationManager.ValidationResults));
                return false;
            }

            return true;
        }

        /**********************************************************************************/

        protected virtual ValidationManager CreateValidationManager()
        {
            return new ValidationManager(IsRuntime ? Payload : null);
        }

        /**********************************************************************************/

        protected virtual Task Validate()
        {
            return Task.FromResult(0);
        }

        /**********************************************************************************/

        protected virtual bool IsInvalidTokenException(Exception ex)
        {
            return false;
        }

        /**********************************************************************************/
        /// <summary>
        /// Creates a suspend request for hub execution
        /// </summary>
        protected void SuspendHubExecution(string message = null)
        {
            SetResponse(ActivityResponse.RequestSuspend, message);
        }

        /**********************************************************************************/
        /// <summary>
        /// Creates a terminate request for hub execution
        /// after that we could stop throwing exceptions on actions
        /// </summary>
        protected void TerminateHubExecution(string message = null)
        {
            SetResponse(ActivityResponse.RequestTerminate, message);
        }

        /**********************************************************************************/
        /// <summary>
        /// Creates a terminate request for hub execution
        /// after that we could stop throwing exceptions on actions
        /// </summary>
        protected void RequestHubExecutionTermination(string message = null)
        {
            TerminateHubExecution(message);
        }

        /**********************************************************************************/
        /// <summary>
        /// returns success to hub
        /// </summary>
        protected void Success(string message = null)
        {
            SetResponse(ActivityResponse.Success, message);
        }

        /**********************************************************************************/

        protected void ExecuteClientActivity(string clientActionName)
        {
            SetResponse(ActivityResponse.ExecuteClientActivity);
            OperationalState.CurrentClientActivityName = clientActionName;
        }

        /**********************************************************************************/
        /// <summary>
        /// skips children of this action
        /// </summary>
        protected void RequestSkipChildren()
        {
            SetResponse(ActivityResponse.SkipChildren);
        }

        /**********************************************************************************/
        /// <summary>
        /// Call an activity or subplan  and return to the current activity
        /// </summary>
        /// <returns></returns>
        protected void RequestCall(Guid targetNodeId)
        {
            SetResponse(ActivityResponse.CallAndReturn);
            OperationalState.CurrentActivityResponse.AddResponseMessageDTO(new ResponseMessageDTO() { Details = targetNodeId });
        }

        /**********************************************************************************/
        /// <summary>
        /// Jumps to an activity that resides in same subplan as current activity
        /// </summary>
        /// <returns></returns>
        protected void RequestJumpToActivity(Guid targetActivityId)
        {
            SetResponse(ActivityResponse.JumpToActivity);
            OperationalState.CurrentActivityResponse.AddResponseMessageDTO(new ResponseMessageDTO() { Details = targetActivityId });
        }

        /**********************************************************************************/
        /// <summary>
        /// Jumps to an activity that resides in same subplan as current activity
        /// </summary>
        /// <returns></returns>
        protected void RequestJumpToSubplan(Guid targetSubplanId)
        {
            SetResponse(ActivityResponse.JumpToSubplan);
            OperationalState.CurrentActivityResponse.AddResponseMessageDTO(new ResponseMessageDTO() { Details = targetSubplanId });
        }

        /**********************************************************************************/
        /// <summary>
        /// Jumps to another plan
        /// </summary>
        /// <returns></returns>
        protected void LaunchPlan(Guid targetPlanId)
        {
            SetResponse(ActivityResponse.LaunchAdditionalPlan);
            OperationalState.CurrentActivityResponse.AddResponseMessageDTO(new ResponseMessageDTO() { Details = targetPlanId });
        }

        /**********************************************************************************/

        protected void SetResponse(ActivityResponse response, string message = null, object details = null)
        {
            OperationalState.CurrentActivityResponse = ActivityResponseDTO.Create(response);

            if (!string.IsNullOrWhiteSpace(message) || details != null)
            {
                OperationalState.CurrentActivityResponse.AddResponseMessageDTO(new ResponseMessageDTO() { Message = message, Details = details });
            }
        }

        /**********************************************************************************/
        /// <summary>
        /// returns error to hub
        /// </summary>
        protected void Error(string errorMessage = null, ActivityErrorCode? errorCode = null)
        {
            SetResponse(ActivityResponse.Error);
            OperationalState.CurrentActivityErrorCode = errorCode;
            OperationalState.CurrentActivityResponse.AddErrorDTO(ErrorDTO.Create(errorMessage, ErrorType.Generic, errorCode.ToString(), null, null, null));
        }

        /**********************************************************************************/
        /// <summary>
        /// returns error to hub
        /// </summary>
        protected void ErrorInvalidToken(string instructionsToUser = null)
        {
            SetResponse(ActivityResponse.Error);
            var errorCode = ActivityErrorCode.AUTH_TOKEN_NOT_PROVIDED_OR_INVALID;
            OperationalState.CurrentActivityErrorCode = errorCode;
            OperationalState.CurrentActivityResponse.AddErrorDTO(ErrorDTO.Create(instructionsToUser, ErrorType.Authentication, errorCode.ToString(), null, null, null));
        }

        /**********************************************************************************/
        /// <summary>
        /// returns error to hub
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="errorCode"></param>
        /// <param name="currentActivity">Activity where the error occured</param>
        /// <param name="currentTerminal">Terminal where the error occured</param>
        /// <returns></returns>
        protected void RaiseError(string errorMessage = null, ActivityErrorCode? errorCode = null, string currentActivity = null, string currentTerminal = null)
        {
            RaiseError(errorMessage, ErrorType.Generic, errorCode, currentActivity, currentTerminal);
        }

        /**********************************************************************************/
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

        /**********************************************************************************/
        /// <summary>
        /// Returns Needs authentication error to hub
        /// </summary>
        /// <returns></returns>
        protected void RaiseNeedsAuthenticationError()
        {
            RaiseError("No AuthToken provided.", ErrorType.Authentication, ActivityErrorCode.AUTH_TOKEN_NOT_PROVIDED_OR_INVALID);
        }

        /**********************************************************************************/
        /// <summary>
        /// Returns authentication error to hub
        /// </summary>
        /// <returns></returns>
        protected void RaiseInvalidTokenError(string instructionsToUser = null)
        {
            RaiseError(instructionsToUser, ErrorType.Authentication, ActivityErrorCode.AUTH_TOKEN_NOT_PROVIDED_OR_INVALID);
        }

        /**********************************************************************************/

        protected virtual Task<DocumentationResponseDTO> GetDocumentation(string documentationType)
        {
            return Task.FromResult(new DocumentationResponseDTO
            {
                Name = "No Documentation found",
                Body = "Please override the GetDocumentation method in the Activity class"
            });
        }

        /**********************************************************************************/

        public async Task<DocumentationResponseDTO> GetDocumentation(ActivityContext activityContext, string documentationType)
        {
            InitializeInternalState(activityContext, null);

            return await GetDocumentation(documentationType);
        }

        /**********************************************************************************/

        #region Legacy helpers
        /// <summary>
        /// Method to be used with Loop Action
        /// Is a helper method to decouple some of the GetCurrentElement Functionality
        /// </summary>
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

        #endregion

        /**********************************************************************************/
    }
}