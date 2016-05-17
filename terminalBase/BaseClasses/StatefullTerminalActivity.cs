using System;
using System.Linq;
using System.Threading.Tasks;
using Fr8Data.Constants;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.DataTransferObjects.Helpers;
using Fr8Data.Manifests;
using Fr8Data.States;
using TerminalBase.Errors;
using TerminalBase.Infrastructure;
using TerminalBase.Interfaces;
using TerminalBase.Models;
using TerminalBase.Services;

namespace TerminalBase.BaseClasses
{
    public abstract class StatefullTerminalActivity : IActivity
    {
        /**********************************************************************************/
        // Declarations
        /**********************************************************************************/
        
        private OperationalStateCM _operationalState;
        private ActivityContext _activityContext;
        private ContainerExecutionContext _containerExecutionContext;

        /**********************************************************************************/

        protected ICrateStorage CurrentPayloadStorage
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

        protected ICrateStorage CurrentActivityStorage => _activityContext.ActivityPayload.CrateStorage;
        protected ActivityPayload CurrentActivity => _activityContext.ActivityPayload;
        protected AuthorizationToken AuthorizationToken => _activityContext.AuthorizationToken;
        protected UpstreamQueryManager UpstreamQueryManager { get; private set; }
        protected ContainerExecutionContext ExecutionContext => _containerExecutionContext;
        protected IHubCommunicator HubCommunicator { get; }
        protected ActivityContext ConfigurationContext => _activityContext;

        /**********************************************************************************/
        // Functions
        /**********************************************************************************/

        protected StatefullTerminalActivity(IHubCommunicator hubCommunicator)
        {
            HubCommunicator = hubCommunicator;
        }

        /**********************************************************************************/

        private void InitializeInternalState(ActivityContext activityContext, ContainerExecutionContext containerExecutionContext)
        {
            _activityContext = activityContext;
            _containerExecutionContext = containerExecutionContext;
            UpstreamQueryManager = new UpstreamQueryManager(activityContext, HubCommunicator);

            if (_containerExecutionContext != null)
            {
                _operationalState = CurrentPayloadStorage.CrateContentsOfType<OperationalStateCM>().FirstOrDefault();

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
            return Task.FromResult(true);
        }

        /**********************************************************************************/

        protected void AddAuthenticationCrate(bool revocation)
        {
            var terminalAuthType = ConfigurationContext.ActivityPayload.ActivityTemplate.Terminal.AuthenticationType;

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

            CurrentActivityStorage.Remove<StandardAuthenticationCM>();
            CurrentActivityStorage.Add(Crate.FromContent("RequiresAuthentication", new StandardAuthenticationCM
            {
                Mode = mode,
                Revocation = revocation
            }));
        }

        /**********************************************************************************/

        public async Task Configure(ActivityContext activityContext)
        {
            try
            {
                InitializeInternalState(activityContext, null);

                if (!await CheckAuthentication())
                {
                    AddAuthenticationCrate(true);
                }

                var configurationType = GetConfigurationRequestType();

                switch (configurationType)
                {
                    case ConfigurationRequestType.Initial:
                        await Initialize();
                        break;

                    case ConfigurationRequestType.Followup:
                        await Configure();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException($"Unsupported configuration type: {configurationType}");
                }
            }
            catch (AuthorizationTokenExpiredOrInvalidException)
            {
                AddAuthenticationCrate(true);
            }
        }

        /**********************************************************************************/

        private void CheckRunTime(string message = "Not available at the design time")
        {
            if (_containerExecutionContext == null)
            {
                throw new InvalidOperationException(message);
            }
        }
        
        /**********************************************************************************/
        
        public Task Activate(ActivityContext activityContext)
        {
            InitializeInternalState(activityContext, null);
            return Activate();
        }

        /**********************************************************************************/

        public Task Deactivate(ActivityContext activityContext)
        {
            InitializeInternalState(activityContext, null);
            return Deactivate();
        }
        
        /**********************************************************************************/

        public Task Run(ActivityContext activityContext, ContainerExecutionContext containerExecutionContext)
        {
            InitializeInternalState(activityContext, containerExecutionContext);

            return Run(RunCurrentActivity);
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
                ErrorInvalidToken("Authorization token is invalid");
                return;
            }
           
            try
            {
                OperationalState.CurrentActivityResponse = null;

                await runMode();

                if (OperationalState.CurrentActivityResponse == null)
                {
                    Success();
                }
            }
            catch (AuthorizationTokenExpiredOrInvalidException ex)
            {
                ErrorInvalidToken(ex.Message);
            }
            catch (ActivityExecutionException ex)
            {
                Error(ex.Message, ex.ErrorCode);
            }
            catch (AggregateException ex)
            {
                Error(ex.Flatten().Message);
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }

        /**********************************************************************************/

        protected virtual ConfigurationRequestType GetConfigurationRequestType()
        {
            return CurrentActivityStorage.Count == 0 ? ConfigurationRequestType.Initial : ConfigurationRequestType.Followup;
        }

        /**********************************************************************************/

        protected abstract Task Initialize();
        protected abstract Task Configure();
        protected abstract Task RunCurrentActivity();

        /**********************************************************************************/

        protected virtual void InitializeInternalState()
        {
        }

        /**********************************************************************************/

        protected virtual Task RunChildActivities()
        {
            return Task.FromResult(0);
        }

        /**********************************************************************************/

        protected virtual Task Activate()
        {
            return Task.FromResult(0);
        }

        /**********************************************************************************/

        protected virtual Task Deactivate()
        {
            return Task.FromResult(0);
        }
        
        /**********************************************************************************/
        /// <summary>
        /// Creates a suspend request for hub execution
        /// </summary>
        protected void RequestHubExecutionSuspension(string message = null)
        {
            SetResponse(ActivityResponse.RequestSuspend, message);
        }

        /**********************************************************************************/
        /// <summary>
        /// Creates a terminate request for hub execution
        /// after that we could stop throwing exceptions on actions
        /// </summary>
        protected void RequestHubExecutionTermination(string message = null)
        {
            SetResponse(ActivityResponse.RequestTerminate, message);
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

        protected void RequestClientActivityExecution(string clientActionName)
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
        protected void RequestLaunchAdditionalPlan(Guid targetPlanId)
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
    }
}
