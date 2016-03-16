using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Constants;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.DataTransferObjects.Helpers;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using TerminalBase.Infrastructure;

namespace TerminalBase.BaseClasses
{
    public abstract class EnhancedTerminalActivity<T> : BaseTerminalActivity
       where T : StandardConfigurationControlsCM, new()
    {
        /**********************************************************************************/
        // Declarations
        /**********************************************************************************/

        private bool _isRunTime;
        private ICrateStorage _currentPayloadStorage;
        private OperationalStateCM _operationalState;

        /**********************************************************************************/

        protected ICrateStorage CurrentPayloadStorage
        {
            get
            {
                CheckRunTime("Payload storage is not available at the design time");
                return _currentPayloadStorage;
            }
            private set
            {
                CheckRunTime("Payload storage is not available at the design time");
                _currentPayloadStorage = value;
            }
        }

        protected OperationalStateCM OperationalState
        {
            get
            {
                CheckRunTime("Operations state is not available at the design time");
                return _operationalState;
            }
            private set
            {
                CheckRunTime("Operations state is not available at the design time");
                _operationalState = value;
            }
        }

        protected bool IsAuthenticationReqiured { get; }
        protected ICrateStorage CurrentActivityStorage { get; private set; }
        protected ActivityDO CurrentActivity { get; private set; }
        protected AuthorizationTokenDO AuthorizationToken { get; private set; }
        protected T ConfigurationControls { get; private set; }
        protected UpstreamNavigator UpstreamNavigator { get; private set; }
        protected UiBuilder UiBuilder { get; private set; }
        protected int LoopIndex => GetLoopIndex(OperationalState, LoopId);
        protected string LoopId => CurrentActivity.GetLoopId();

        /**********************************************************************************/
        // Functions
        /**********************************************************************************/

        protected EnhancedTerminalActivity(bool isAuthenticationRequired)
        {
            IsAuthenticationReqiured = isAuthenticationRequired;
            UiBuilder = new UiBuilder();
        }

        /**********************************************************************************/

        public sealed override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            if (IsAuthenticationReqiured)
            {
                CheckAuthentication(authTokenDO);
            }

            AuthorizationToken = authTokenDO;
            CurrentActivity = curActivityDO;

            UpstreamNavigator = new UpstreamNavigator(CurrentActivity, HubCommunicator, CurrentFr8UserId);
            
            using (var storage = CrateManager.GetUpdatableStorage(CurrentActivity))
            {
                CurrentActivityStorage = storage;

                var configurationType = GetConfigurationRequestType();

                switch (configurationType)
                {
                    case ConfigurationRequestType.Initial:
                        await InitialConfiguration();
                        break;

                    case ConfigurationRequestType.Followup:
                        await FollowupConfiguration();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException($"Unsupported configuration type: {configurationType}");
                }
            }

            return curActivityDO;
        }

        /**********************************************************************************/

        private void CheckRunTime(string message = "Not available at the design time")
        {
            if (!_isRunTime)
            {
                throw new InvalidOperationException(message);
            }
        }
        
        /**********************************************************************************/

        private async Task InitialConfiguration()
        {
            ConfigurationControls = CrateConfigurationControls();
            CurrentActivityStorage.Clear();

            CurrentActivityStorage.Add(Crate.FromContent(ConfigurationControlsLabel, ConfigurationControls, AvailabilityType.Configuration));

            await Initialize();

            SyncConfControlsBack();
        }

        /**********************************************************************************/

        private async Task FollowupConfiguration()
        {
            SyncConfControls();

            if (await Validate())
            {
                await Configure();
            }

            SyncConfControlsBack();
        }

        /**********************************************************************************/

        public sealed override async Task<ActivityDO> Activate(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            if (IsAuthenticationReqiured)
            {
                CheckAuthentication(authTokenDO);
            }

            AuthorizationToken = authTokenDO;
            CurrentActivity = curActivityDO;

            UpstreamNavigator = new UpstreamNavigator(CurrentActivity, HubCommunicator, CurrentFr8UserId);

            using (var storage = CrateManager.GetUpdatableStorage(CurrentActivity))
            {
                CurrentActivityStorage = storage;

                SyncConfControls();

                if (await Validate())
                {
                    await Activate();
                }
            }

            return curActivityDO;
        }

        /**********************************************************************************/

        public sealed override async Task<ActivityDO> Deactivate(ActivityDO curActivityDO)
        {
            CurrentActivity = curActivityDO;

            UpstreamNavigator = new UpstreamNavigator(CurrentActivity, HubCommunicator, CurrentFr8UserId);

            using (var storage = CrateManager.GetUpdatableStorage(CurrentActivity))
            {
                CurrentActivityStorage = storage;
                SyncConfControls();

                await Deactivate();
            }

            return curActivityDO;
        }

        /**********************************************************************************/

        public sealed override Task<PayloadDTO> ExecuteChildActivities(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            return Run(curActivityDO, containerId, authTokenDO, ChildActivitiesExecuted);
        }

        /**********************************************************************************/

        public Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            return Run(curActivityDO, containerId, authTokenDO, Run);
        }

        /**********************************************************************************/

        private async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO, Func<Task> runCore)
        {
            var processPayload = await HubCommunicator.GetPayload(curActivityDO, containerId, CurrentFr8UserId);

            if (IsAuthenticationReqiured && NeedsAuthentication(authTokenDO))
            {
                return NeedsAuthenticationError(processPayload);
            }

            AuthorizationToken = authTokenDO;
            CurrentActivity = curActivityDO;
            
            UpstreamNavigator = new UpstreamNavigator(CurrentActivity, HubCommunicator, CurrentFr8UserId);

            _isRunTime = true;

            using (var payloadstorage = CrateManager.GetUpdatableStorage(processPayload))
            using (var actionStoragestorage = CrateManager.GetUpdatableStorage(CurrentActivity))
            {
                CurrentActivityStorage = actionStoragestorage;
                CurrentPayloadStorage = payloadstorage;
                OperationalState = CurrentPayloadStorage.CrateContentsOfType<OperationalStateCM>().FirstOrDefault();

                if (OperationalState == null)
                {
                    throw new InvalidOperationException("Operational state crate is not found");
                }

                SyncConfControls();

                try
                {
                    if (!await Validate())
                    {
                        Error("Activity was incorrectly configured");
                        return processPayload;
                    }

                    await runCore();

                    Success();
                }
                catch (ActionExecutionException ex)
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

            return processPayload;
        }
        
        /**********************************************************************************/

        protected virtual T CrateConfigurationControls()
        {
            return new T();
        }

        /**********************************************************************************/

        protected virtual ConfigurationRequestType GetConfigurationRequestType()
        {
            return CurrentActivityStorage.Count == 0 ? ConfigurationRequestType.Initial : ConfigurationRequestType.Followup;
        }

        /**********************************************************************************/

        protected virtual Task Initialize()
        {
            return Task.FromResult(0);
        }

        /**********************************************************************************/

        protected virtual Task Configure()
        {
            return Task.FromResult(0);
        }

        /**********************************************************************************/

        protected virtual Task Run()
        {
            return Task.FromResult(0);
        }

        /**********************************************************************************/

        protected virtual Task ChildActivitiesExecuted()
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

        protected virtual Task<bool> Validate()
        {
            return Task.FromResult(true);
        }

        /**********************************************************************************/

        private void SyncConfControls()
        {
            var ui = CurrentActivityStorage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (ui == null)
            {
                throw new InvalidOperationException("Configuration controls crate is missing");
            }

            ConfigurationControls = CrateConfigurationControls();
            ConfigurationControls.SyncWith(ui);
        }

        /**********************************************************************************/

        private void SyncConfControlsBack()
        {
            CurrentActivityStorage.Remove<StandardConfigurationControlsCM>();
            // we clone ConfigurationControls.Controls to remove possible custom properties from ActionUi from serialization
            CurrentActivityStorage.Add(Crate.FromContent(ConfigurationControlsLabel, new StandardConfigurationControlsCM(ConfigurationControls.Controls), AvailabilityType.Configuration));
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
        protected void SkipChildren()
        {
            SetResponse(ActivityResponse.SkipChildren);
        }

        /**********************************************************************************/
        /// <summary>
        /// Creates a reprocess child actions request
        /// </summary>
        protected void ReprocessChildren()
        {
            SetResponse(ActivityResponse.ReProcessChildren);
        }

        /**********************************************************************************/
        /// <summary>
        /// Jumps to an activity that resides in same subplan as current activity
        /// </summary>
        /// <returns></returns>
        protected void JumpToActivity(Guid targetActivityId)
        {
            SetResponse(ActivityResponse.JumpToActivity);
            OperationalState.CurrentActivityResponse.AddResponseMessageDTO(new ResponseMessageDTO() {Details = targetActivityId});
        }

        /**********************************************************************************/
        /// <summary>
        /// Jumps to an activity that resides in same subplan as current activity
        /// </summary>
        /// <returns></returns>
        protected void JumpToSubplan(Guid targetSubplanId)
        {
            SetResponse(ActivityResponse.JumpToSubplan);
            OperationalState.CurrentActivityResponse.AddResponseMessageDTO(new ResponseMessageDTO() { Details = targetSubplanId });
        }

        /**********************************************************************************/

        protected void SetResponse(ActivityResponse response, string message = null)
        {
            OperationalState.CurrentActivityResponse = ActivityResponseDTO.Create(response);

            if (!string.IsNullOrWhiteSpace(message))
            {
                OperationalState.CurrentActivityResponse.AddResponseMessageDTO(new ResponseMessageDTO() {Message = message});
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
        // we don't want uncontrollable extensibility
        protected sealed override Task<ICrateStorage> ValidateActivity(ActivityDO curActivityDO)
        {
            return base.ValidateActivity(curActivityDO);
        }

        public sealed override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            return base.ConfigurationEvaluator(curActivityDO);
        }

        protected sealed override Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            return base.InitialConfigurationResponse(curActivityDO, authTokenDO);
        }

        protected sealed override Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            return base.FollowupConfigurationResponse(curActivityDO, authTokenDO);
        }

        protected sealed override Crate MergeUpstreamFields<TManifest>(ActivityDO curActivityDO, string label, FieldDTO[] upstreamFields)
        {
            return base.MergeUpstreamFields<TManifest>(curActivityDO, label, upstreamFields);
        }

        public sealed override Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(ActivityDO activityDO, CrateDirection direction)
        {
            return base.GetCratesByDirection<TManifest>(activityDO, direction);
        }

        public sealed override Task<List<Crate>> GetCratesByDirection(ActivityDO activityDO, CrateDirection direction)
        {
            return base.GetCratesByDirection(activityDO, direction);
        }

        public sealed override Task<FieldDescriptionsCM> GetDesignTimeFields(Guid activityId, CrateDirection direction, AvailabilityType availability = AvailabilityType.NotSet)
        {
            return base.GetDesignTimeFields(activityId, direction, availability);
        }

        public sealed override Task<FieldDescriptionsCM> GetDesignTimeFields(ActivityDO activityDO, CrateDirection direction, AvailabilityType availability = AvailabilityType.NotSet)
        {
            return base.GetDesignTimeFields(activityDO, direction, availability);
        }

        public sealed override Task<List<CrateManifestType>> BuildUpstreamManifestList(ActivityDO activityDO)
        {
            return base.BuildUpstreamManifestList(activityDO);
        }

        public sealed override Task<List<string>> BuildUpstreamCrateLabelList(ActivityDO activityDO)
        {
            return base.BuildUpstreamCrateLabelList(activityDO);
        }

        public sealed override Task<Crate<FieldDescriptionsCM>> GetUpstreamManifestListCrate(ActivityDO activityDO)
        {
            return base.GetUpstreamManifestListCrate(activityDO);
        }

        public sealed override Task<Crate<FieldDescriptionsCM>> GetUpstreamCrateLabelListCrate(ActivityDO activityDO)
        {
            return base.GetUpstreamCrateLabelListCrate(activityDO);
        }

        protected sealed override Task<List<Crate<StandardFileDescriptionCM>>> GetUpstreamFileHandleCrates(ActivityDO activityDO)
        {
            return base.GetUpstreamFileHandleCrates(activityDO);
        }

        protected sealed override Task<Crate> MergeUpstreamFields<TManifest>(ActivityDO curActivityDO, string label)
        {
            return base.MergeUpstreamFields<TManifest>(curActivityDO, label);
        }

        protected sealed override Task<FieldDTO[]> GetCratesFieldsDTO<TManifest>(ActivityDO curActivityDO, CrateDirection crateDirection)
        {
            return base.GetCratesFieldsDTO<TManifest>(curActivityDO, crateDirection);
        }

        /**********************************************************************************/
    }
}
