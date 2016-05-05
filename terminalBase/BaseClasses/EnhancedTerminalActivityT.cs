using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8Data.Constants;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.DataTransferObjects.Helpers;
using Fr8Data.Helpers;
using Fr8Data.Manifests;
using Fr8Data.States;
using Hub.Managers;
using TerminalBase.Infrastructure;
using Newtonsoft.Json.Linq;
using TerminalBase.Errors;

namespace TerminalBase.BaseClasses
{
    public abstract class EnhancedTerminalActivity<T> : BaseTerminalActivity
       where T : StandardConfigurationControlsCM
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

        protected bool IsAuthenticationRequired { get; }
        protected ICrateStorage CurrentActivityStorage { get; private set; }
        protected ActivityDTO CurrentActivity { get; private set; }
        protected AuthorizationTokenDTO AuthorizationToken { get; private set; }
        protected T ConfigurationControls { get; private set; }
        protected UpstreamQueryManager UpstreamQueryManager { get; private set; }
        protected UiBuilder UiBuilder { get; private set; }
        protected int LoopIndex => GetLoopIndex(OperationalState);

        /**********************************************************************************/
        // Functions
        /**********************************************************************************/


        protected EnhancedTerminalActivity(bool isAuthenticationRequired)
        {
            IsAuthenticationRequired = isAuthenticationRequired;
            UiBuilder = new UiBuilder();
            ActivityName = GetType().Name;
        } 

        /**********************************************************************************/

        private bool AuthorizeIfNecessary(ActivityDTO activityDTO, AuthorizationTokenDTO authTokenDTO)
        {
            if (IsAuthenticationRequired)
            {
                return CheckAuthentication(activityDTO, authTokenDTO);
            }

            return false;
        }

        protected virtual bool IsTokenInvalidation(Exception ex)
        {
            return false;
        }

        /**********************************************************************************/

        public sealed override async Task<ActivityDTO> Configure(ActivityDTO curActivityDTO, AuthorizationTokenDTO authTokenDTO)
        {
            try
            {
                if (AuthorizeIfNecessary(curActivityDTO, authTokenDTO))
                {
                    return curActivityDTO;
                }

                AuthorizationToken = authTokenDTO;
                CurrentActivity = curActivityDTO;

                UpstreamQueryManager = new UpstreamQueryManager(CurrentActivity, HubCommunicator, CurrentFr8UserId);

                using (var storage = CrateManager.GetUpdatableStorage(CurrentActivity))
                {
                    CurrentActivityStorage = storage;

                    var configurationType = GetConfigurationRequestType();
                    var runtimeCrateManager = new RuntimeCrateManager(CurrentActivityStorage, CurrentActivity.Label);

                    switch (configurationType)
                    {
                        case ConfigurationRequestType.Initial:
                            await InitialConfiguration(runtimeCrateManager);
                            break;

                        case ConfigurationRequestType.Followup:
                            await FollowupConfiguration(runtimeCrateManager);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException($"Unsupported configuration type: {configurationType}");
                    }
                }

                return curActivityDTO;
            }
            catch (Exception ex)
            {
                if (IsTokenInvalidation(ex))
                {
                    AddAuthenticationCrate(curActivityDTO, true);
                    return curActivityDTO;
                }

                throw;
            }
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

        private async Task InitialConfiguration(RuntimeCrateManager runtimeCrateManager)
        {
            ConfigurationControls = CrateConfigurationControls();
            CurrentActivityStorage.Clear();

            CurrentActivityStorage.Add(Crate.FromContent(ConfigurationControlsLabel, ConfigurationControls, AvailabilityType.Configuration));

            await Initialize(runtimeCrateManager);

            SyncConfControlsBack();
        }

        /**********************************************************************************/

        private async Task FollowupConfiguration(RuntimeCrateManager runtimeCrateManager)
        {
            SyncConfControls();

            if (await Validate())
            {
                await Configure(runtimeCrateManager);
            }

            SyncConfControlsBack();
        }

        /**********************************************************************************/

        public sealed override async Task<ActivityDTO> Activate(ActivityDTO curActivityDTO, AuthorizationTokenDTO authTokenDTO)
        {
            if (AuthorizeIfNecessary(curActivityDTO, authTokenDTO))
            {
                return curActivityDTO;
            }

            AuthorizationToken = authTokenDTO;
            CurrentActivity = curActivityDTO;

            UpstreamQueryManager = new UpstreamQueryManager(CurrentActivity, HubCommunicator, CurrentFr8UserId);

            using (var storage = CrateManager.GetUpdatableStorage(CurrentActivity))
            {
                CurrentActivityStorage = storage;

                SyncConfControls();

                if (await Validate())
                {
                    await Activate();
                }
            }

            return curActivityDTO;
        }

        /**********************************************************************************/

        public sealed override async Task<ActivityDTO> Deactivate(ActivityDTO curActivityDTO)
        {
            CurrentActivity = curActivityDTO;

            UpstreamQueryManager = new UpstreamQueryManager(CurrentActivity, HubCommunicator, CurrentFr8UserId);

            using (var storage = CrateManager.GetUpdatableStorage(CurrentActivity))
            {
                CurrentActivityStorage = storage;
                SyncConfControls();

                await Deactivate();
            }

            return curActivityDTO;
        }

        /**********************************************************************************/

        public sealed override Task<PayloadDTO> ExecuteChildActivities(ActivityDTO curActivityDTO, Guid containerId, AuthorizationTokenDTO authTokenDTO)
        {
            return Run(curActivityDTO, containerId, authTokenDTO, RunChildActivities);
        }

        /**********************************************************************************/

        public Task<PayloadDTO> Run(ActivityDTO curActivityDTO, Guid containerId, AuthorizationTokenDTO authTokenDTO)
        {
            return Run(curActivityDTO, containerId, authTokenDTO, RunCurrentActivity);
        }

        /**********************************************************************************/

        private async Task<PayloadDTO> Run(ActivityDTO curActivityDTO, Guid containerId, AuthorizationTokenDTO authTokenDTO, Func<Task> runMode)
        {
            var processPayload = await HubCommunicator.GetPayload(curActivityDTO, containerId, CurrentFr8UserId);

            if (IsAuthenticationRequired && NeedsAuthentication(authTokenDTO))
            {
                return NeedsAuthenticationError(processPayload);
            }

            AuthorizationToken = authTokenDTO;
            CurrentActivity = curActivityDTO;
            
            UpstreamQueryManager = new UpstreamQueryManager(CurrentActivity, HubCommunicator, CurrentFr8UserId);

            _isRunTime = true;

            var activityStorageContents = CurrentActivity.CrateStorage;
            using (var payloadstorage = CrateManager.GetUpdatableStorage(processPayload))
            using (var activityStorage = CrateManager.GetUpdatableStorage(CurrentActivity))
            {
                CurrentActivityStorage = activityStorage;
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
            try
            {
                var previousActivityStorage = JToken.Parse(activityStorageContents);
                var currentActivityStorage = JToken.Parse(CurrentActivity.CrateStorage);
                if (!JToken.DeepEquals(previousActivityStorage, currentActivityStorage))
                {
                    await HubCommunicator.SaveActivity(CurrentActivity, CurrentFr8UserId);
                }
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
            return processPayload;
        }
        
        /**********************************************************************************/

        protected T AssignNamesForUnnamedControls(T configurationControls)
        {
            int controlId = 0;
            var controls = configurationControls.EnumerateControlsDefinitions();

            foreach (var controlDefinition in controls)
            {
                if (string.IsNullOrWhiteSpace(controlDefinition.Name))
                {
                    controlDefinition.Name = controlDefinition.GetType().Name + controlId++;
                }
            }

            return configurationControls;
        }
        
        /**********************************************************************************/

        protected virtual T CrateConfigurationControls()
        {
            var uiBuilderConstructor = typeof (T).GetConstructor(new [] {typeof (UiBuilder)});

            if (uiBuilderConstructor != null)
            {
                return AssignNamesForUnnamedControls((T) uiBuilderConstructor.Invoke(new object[] {UiBuilder}));
            }

            var defaultConstructor = typeof (T).GetConstructor(new Type[0]);

            if (defaultConstructor == null)
            {
                throw new InvalidOperationException($"Unable to find default constructor or constructor accepting UiBuilder for type {typeof(T).FullName}");
            }

            return AssignNamesForUnnamedControls((T)defaultConstructor.Invoke(null));
        }

        /**********************************************************************************/

        protected virtual ConfigurationRequestType GetConfigurationRequestType()
        {
            return CurrentActivityStorage.Count == 0 ? ConfigurationRequestType.Initial : ConfigurationRequestType.Followup;
        }

        /**********************************************************************************/

        protected abstract Task Initialize(RuntimeCrateManager runtimeCrateManager);
        protected abstract Task Configure(RuntimeCrateManager runtimeCrateManager);

        /**********************************************************************************/

        protected abstract Task RunCurrentActivity();

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

        protected virtual Task<bool> Validate()
        {
            return Task.FromResult(true);
        }

        private const string ConfigurationValuesCrateLabel = "Configuration Values";
        /// <summary>
        /// Get or sets value of configuration field with the given key stored in current activity storage
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected string this[string key]
        {
            get
            {
                CheckCurrentActivityStorageAvailability();
                var crate = CurrentActivityStorage.FirstCrateOrDefault<FieldDescriptionsCM>(x => x.Label == ConfigurationValuesCrateLabel);
                return crate?.Content.Fields.FirstOrDefault(x => x.Key == key)?.Value;
            }
            set
            {
                CheckCurrentActivityStorageAvailability();
                var crate = CurrentActivityStorage.FirstCrateOrDefault<FieldDescriptionsCM>(x => x.Label == ConfigurationValuesCrateLabel);
                if (crate == null)
                {
                    crate = Crate<FieldDescriptionsCM>.FromContent(ConfigurationValuesCrateLabel, new FieldDescriptionsCM(), AvailabilityType.Configuration);
                    CurrentActivityStorage.Add(crate);
                }
                var field = crate.Content.Fields.FirstOrDefault(x => x.Key == key);
                if (field == null)
                {
                    field = new FieldDTO(key, AvailabilityType.Configuration);
                    crate.Content.Fields.Add(field);
                }
                field.Value = value;
                CurrentActivityStorage.ReplaceByLabel(crate);
            }
        }

        private void CheckCurrentActivityStorageAvailability()
        {
            if (CurrentActivityStorage == null)
            {
                throw new ApplicationException("Current activity storage is not available");
            }
        }

        /**********************************************************************************/
        // SyncConfControls and SyncConfControlsBack are pair of methods that serves the following reason:
        // We want to work with StandardConfigurationControlsCM in form of ActivityUi that has handy properties to directly access certain controls
        // But when we deserialize activity's crate storage we get StandardConfigurationControlsCM. So we need a way to 'convert' StandardConfigurationControlsCM
        // from crate storage to ActivityUI.
        // SyncConfControls takes properties of controls in StandardConfigurationControlsCM from activity's storage and copies them into ActivityUi.
        private void SyncConfControls()
        {
            var ui = CurrentActivityStorage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (ui == null)
            {
                throw new InvalidOperationException("Configuration controls crate is missing");
            }

            ConfigurationControls = CrateConfigurationControls();
            ConfigurationControls.SyncWith(ui);
            
            if (ui.Controls != null)
            {
                var dynamicControlsCollection = Fr8ReflectionHelper.GetMembers(ConfigurationControls.GetType()).Where(x => x.CanRead && x.GetCustomAttribute<DynamicControlsAttribute>() != null && CheckIfMemberIsControlsCollection(x)).ToDictionary(x => x.Name, x => x);

                if (dynamicControlsCollection.Count > 0)
                {
                    foreach (var control in ui.Controls)
                    {
                        if (string.IsNullOrWhiteSpace(control.Name))
                        {
                            continue;
                        }

                        var delim = control.Name.IndexOf('_');

                        if (delim <= 0)
                        {
                            continue;
                        }

                        var prefix = control.Name.Substring(0, delim);
                        IMemberAccessor member;

                        if (!dynamicControlsCollection.TryGetValue(prefix, out member))
                        {
                            continue;
                        }

                        var controlsCollection = (IList)member.GetValue(ConfigurationControls);

                        if (controlsCollection == null && (!member.CanWrite || member.MemberType.IsAbstract || member.MemberType.IsInterface))
                        {
                            continue;
                        }

                        if (controlsCollection == null)
                        {
                            controlsCollection = (IList)Activator.CreateInstance(member.MemberType);
                            member.SetValue(ConfigurationControls, controlsCollection);
                        }

                        control.Name = control.Name.Substring(delim + 1);
                        controlsCollection.Add(control);
                    }
                }
            }
        }

        /**********************************************************************************/

        private static bool CheckIfMemberIsControlsCollection(IMemberAccessor member)
        {
            if (member.MemberType.IsInterface && CheckIfTypeIsControlsCollection(member.MemberType))
            {
                return true;
            }

            foreach (var @interface in member.MemberType.GetInterfaces())
            {
                if (CheckIfTypeIsControlsCollection(@interface))
                {
                    return true;
                }
            }

            return false;
        }

        /**********************************************************************************/

        private static bool CheckIfTypeIsControlsCollection(Type type)
        {
            if (type.IsGenericType)
            {
                var genericTypeDef = type.GetGenericTypeDefinition();

                if (typeof(IList<>) == genericTypeDef)
                {
                    if (typeof(IControlDefinition).IsAssignableFrom(type.GetGenericArguments()[0]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        /**********************************************************************************/
        // Activity logic can update state of configuration controls. But as long we have copied  configuration controls crate from the storage into 
        // new instance of ActivityUi changes made to ActivityUi won't be reflected in storage.
        // we have to persist in storage changes we've made to ActivityUi.
        // we do this in the most simple way by replacing old StandardConfigurationControlsCM with ActivityUi.
        private void SyncConfControlsBack()
        {
            CurrentActivityStorage.Remove<StandardConfigurationControlsCM>();
            // we create new StandardConfigurationControlsCM with controls from ActivityUi.
            // We do this because ActivityUi can has properties to access specific controls. We don't want those propeties exist in serialized crate.

            var configurationControlsToAdd = new StandardConfigurationControlsCM(ConfigurationControls.Controls);
            CurrentActivityStorage.Add(Crate.FromContent(ConfigurationControlsLabel, configurationControlsToAdd, AvailabilityType.Configuration));

            int insertIndex = 0;

            foreach (var member in Fr8ReflectionHelper.GetMembers(ConfigurationControls.GetType()).Where(x => x.CanRead))
            {
                if (member.GetCustomAttribute<DynamicControlsAttribute>() != null && CheckIfMemberIsControlsCollection(member))
                {
                    var collection = member.GetValue(ConfigurationControls) as IList;

                    if (collection != null)
                    {
                        for (int index = 0; index < collection.Count; index++)
                        {
                            var control = collection[index] as ControlDefinitionDTO;

                            if (control != null)
                            {
                                control.Name = member.Name + "_" + control.Name;
                                configurationControlsToAdd.Controls.Insert(insertIndex, control);
                                insertIndex++;
                            }
                        }
                    }
                }

                var controlDef = member.GetValue(ConfigurationControls) as IControlDefinition;
                if (!string.IsNullOrWhiteSpace(controlDef?.Name))
                {
                    for (int i = 0; i < configurationControlsToAdd.Controls.Count; i++)
                    {
                        if (configurationControlsToAdd.Controls[i].Name == controlDef.Name)
                        {
                            insertIndex = i + 1;
                            break;
                        }
                    }
                }
            }
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
            OperationalState.CurrentActivityResponse.AddResponseMessageDTO(new ResponseMessageDTO() {Details = targetActivityId});
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
                OperationalState.CurrentActivityResponse.AddResponseMessageDTO(new ResponseMessageDTO() {Message = message, Details = details});
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
        // we don't want uncontrollable extensibility
        protected sealed override Task<ICrateStorage> ValidateActivity(ActivityDTO curActivityDTO)
        {
            return base.ValidateActivity(curActivityDTO);
        }

        public sealed override ConfigurationRequestType ConfigurationEvaluator(ActivityDTO curActivityDTO)
        {
            return base.ConfigurationEvaluator(curActivityDTO);
        }

        protected sealed override Task<ActivityDTO> InitialConfigurationResponse(ActivityDTO curActivityDTO, AuthorizationTokenDTO authTokenDTO)
        {
            return base.InitialConfigurationResponse(curActivityDTO, authTokenDTO);
        }

        protected sealed override Task<ActivityDTO> FollowupConfigurationResponse(ActivityDTO curActivityDTO, AuthorizationTokenDTO authTokenDTO)
        {
            return base.FollowupConfigurationResponse(curActivityDTO, authTokenDTO);
        }

        public sealed override Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(ActivityDTO activityDO, CrateDirection direction)
        {
            return base.GetCratesByDirection<TManifest>(activityDO, direction);
        }

        public sealed override Task<List<Crate>> GetCratesByDirection(ActivityDTO activityDO, CrateDirection direction)
        {
            return base.GetCratesByDirection(activityDO, direction);
        }

        public sealed override Task<FieldDescriptionsCM> GetDesignTimeFields(ActivityDTO activityDO, CrateDirection direction, AvailabilityType availability = AvailabilityType.NotSet)
        {
            return base.GetDesignTimeFields(activityDO, direction, availability);
        }

        public sealed override Task<List<CrateManifestType>> BuildUpstreamManifestList(ActivityDTO activityDO)
        {
            return base.BuildUpstreamManifestList(activityDO);
        }

        public sealed override Task<List<string>> BuildUpstreamCrateLabelList(ActivityDTO activityDO)
        {
            return base.BuildUpstreamCrateLabelList(activityDO);
        }

        public sealed override Task<Crate<FieldDescriptionsCM>> GetUpstreamManifestListCrate(ActivityDTO activityDO)
        {
            return base.GetUpstreamManifestListCrate(activityDO);
        }

        public sealed override Task<Crate<FieldDescriptionsCM>> GetUpstreamCrateLabelListCrate(ActivityDTO activityDO)
        {
            return base.GetUpstreamCrateLabelListCrate(activityDO);
        }

        protected sealed override Task<List<Crate<StandardFileDescriptionCM>>> GetUpstreamFileHandleCrates(ActivityDTO activityDO)
        {
            return base.GetUpstreamFileHandleCrates(activityDO);
        }

        /**********************************************************************************/
    }
}
