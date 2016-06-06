using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using Fr8Data.States;
using TerminalBase.Helpers;
using TerminalBase.Infrastructure;
using TerminalBase.Models;
using TerminalBase.Services;

namespace TerminalBase.BaseClasses
{
    // Common helper stuff used in currently implemented activities
    public abstract class BaseTerminalActivityLegacy : StatefullTerminalActivity
    {
        public const string ConfigurationControlsLabel = "Configuration_Controls";
        public const string ValidationCrateLabel = "Validation Results";

        protected ICrateManager CrateManager { get; private set; }
        public CrateSignaller CrateSignaller { get; set; }
        protected bool IsAuthenticationRequired { get; }
        protected bool DisableValidationOnFollowup { get; set; }
        private PlanHelper _planHelper;
        private ControlHelper _controlHelper;
        private readonly BaseTerminalEvent _eventLogger;


        protected int LoopIndex => GetLoopIndex();
        protected ControlHelper ControlHelper => _controlHelper ?? (_controlHelper = new ControlHelper(ActivityContext));
        protected ValidationManager ValidationManager { get; set; }
        protected PlanHelper PlanHelper => _planHelper ?? (_planHelper = new PlanHelper(HubCommunicator));
        protected Guid ActivityId => ActivityContext.ActivityPayload.Id;
        protected string CurrentUserId => ActivityContext.UserId;
        protected void SendEventReport(string message) => _eventLogger.SendEventReport(MyTemplate.Terminal.Name, message);
        protected UpstreamQueryManager UpstreamQueryManager { get; private set; }

        protected abstract ActivityTemplateDTO MyTemplate { get; }

        protected BaseTerminalActivityLegacy(ICrateManager crateManager)
        {
            _eventLogger = new BaseTerminalEvent();
            CrateManager = crateManager;
            IsAuthenticationRequired = MyTemplate.NeedsAuthentication;
        }

        protected override void InitializeInternalState()
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
            UpstreamQueryManager = new UpstreamQueryManager(ActivityContext, HubCommunicator);
        }

        protected override async Task<bool> BeforeRun()
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

        protected override async Task<bool> BeforeActivate()
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

        protected override async Task<bool> BeforeConfigure(ConfigurationRequestType configurationRequestType)
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

        protected virtual ValidationManager CreateValidationManager()
        {
            return new ValidationManager(IsRuntime ? Payload : null);
        }

        protected virtual Task Validate()
        {
            return Task.FromResult(0);
        }

        protected override Task<bool> CheckAuthentication()
        {
            return Task.FromResult(!IsAuthenticationRequired || !NeedsAuthentication());
        }

        public virtual bool NeedsAuthentication()
        {
            return string.IsNullOrEmpty(AuthorizationToken?.Token);
        }

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

        protected async Task<ActivityPayload> ConfigureChildActivity(ActivityPayload parent, ActivityPayload child)
        {
            var result = await HubCommunicator.ConfigureActivity(child);
            parent.ChildrenActivities.Remove(child);
            parent.ChildrenActivities.Add(result);
            return result;
        }

        protected async Task<ActivityPayload> AddAndConfigureChildActivity(Guid parentActivityId, ActivityTemplateDTO activityTemplate, string name = null, string label = null, int? order = null)
        {
            //assign missing properties
            label = string.IsNullOrEmpty(label) ? activityTemplate.Label : label;
            name = string.IsNullOrEmpty(name) ? activityTemplate.Label : label;
            return await HubCommunicator.CreateAndConfigureActivity(activityTemplate.Id, name, order, parentActivityId);
        }

        protected async Task<ActivityPayload> AddAndConfigureChildActivity(ActivityPayload parentActivity, ActivityTemplateDTO activityTemplate, string name = null, string label = null, int? order = null)
        {
            var child = await AddAndConfigureChildActivity(parentActivity.Id, activityTemplate, name, label, order);
            parentActivity.ChildrenActivities.Add(child);
            return child;
        }

        protected async Task<ActivityTemplateDTO> GetActivityTemplate(Guid activityTemplateId)
        {
            var allActivityTemplates = await HubCommunicator.GetActivityTemplates();

            var foundActivity = allActivityTemplates.FirstOrDefault(a => a.Id == activityTemplateId);


            if (foundActivity == null)
            {
                throw new Exception($"ActivityTemplate was not found. Id: {activityTemplateId}");
            }

            return foundActivity;
        }

        protected async Task<ActivityTemplateDTO> GetActivityTemplate(string terminalName, string activityTemplateName, string activityTemplateVersion = "1", string terminalVersion = "1")
        {
            var allActivityTemplates = await HubCommunicator.GetActivityTemplates();

            var foundActivity = allActivityTemplates.FirstOrDefault(a =>
                a.Terminal.Name == terminalName && a.Terminal.Version == terminalVersion &&
                a.Name == activityTemplateName && a.Version == activityTemplateVersion);

            if (foundActivity == null)
            {
                throw new Exception($"ActivityTemplate was not found. TerminalName: {terminalName}\nTerminalVersion: {terminalVersion}\nActivitiyTemplateName: {activityTemplateName}\nActivityTemplateVersion: {activityTemplateVersion}");
            }

            return foundActivity;
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
            await HubCommunicator.NotifyUser(notificationMsg);
        }

        protected void AddAdvisoryCrate(string name, string content)
        {
            var advisoryCrate = Storage.CratesOfType<AdvisoryMessagesCM>().FirstOrDefault();
            var currentAdvisoryResults = advisoryCrate == null ? new AdvisoryMessagesCM() : advisoryCrate.Content;

            var advisory = currentAdvisoryResults.Advisories.FirstOrDefault(x => x.Name == name);

            if (advisory == null)
            {
                currentAdvisoryResults.Advisories.Add(new AdvisoryMessageDTO { Name = name, Content = content });
            }
            else
            {
                advisory.Content = content;
            }

            Storage.Add(Crate.FromContent("Advisories", currentAdvisoryResults));
        }

        public DocumentationResponseDTO GetDefaultDocumentation(string solutionName, double solutionVersion, string terminalName, string body)
        {
            var curSolutionPage = new DocumentationResponseDTO
            {
                Name = solutionName,
                Version = solutionVersion,
                Terminal = terminalName,
                Body = body
            };

            return curSolutionPage;
        }

        public DocumentationResponseDTO GenerateErrorResponse(string errorMessage)
        {
            return new DocumentationResponseDTO
            {
                Body = errorMessage,
                //Type = ActivityResponse.ShowDocumentation.ToString()
            };
        }


        public DocumentationResponseDTO GenerateDocumentationResponse(string documentation)
        {
            return new DocumentationResponseDTO
            {
                Body = documentation,
                //Type = ActivityResponse.ShowDocumentation.ToString()
            };
        }

        #endregion 
    }
}