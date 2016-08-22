using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Utilities;
using Fr8.Infrastructure.Utilities.Logging;
using Hub.Interfaces;
using Hub.Managers;
using Fr8.Infrastructure.Data.States;

namespace Hub.Services
{
    // The class purpose is to allow our manifest registry submission to be represented as internal Fr8 plan. The plan consists of the following steps:
    // 1. Start monitoring responses from particular Google Form (form belongs to account fr8test1@gmail.com and is selected based on current environment)
    // 2. When response is received use its fields to compose a message
    // 3. Create a JIRA issue with built message as description and assign it to specific user (Admin in our case)
    public class ManifestRegistryMonitor : IManifestRegistryMonitor
    {
        private readonly IActivity _activity;
        private readonly ICrateManager _crateManager;
        private readonly IPlan _plan;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IFr8Account _fr8Account;
        private readonly IConfigRepository _configRepository;

        private const string MonitoringPlanName = "Monitoring Manifest Submissions";
        private const string MessageName = "JIRA description";
        private const string ManifestMonitoringPrefix = "Manifest Monitoring - ";

        private int _isRunning;

        private TaskCompletionSource<ManifestRegistryMonitorResult> _currentRun;

        public ManifestRegistryMonitor(
            IActivity activity,
            ICrateManager crateManager,
            IPlan plan,
            IUnitOfWorkFactory unitOfWorkFactory,
            IFr8Account fr8Account,
            IConfigRepository configRepository)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }
            if (crateManager == null)
            {
                throw new ArgumentNullException(nameof(crateManager));
            }
            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }
            if (unitOfWorkFactory == null)
            {
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
            }
            if (fr8Account == null)
            {
                throw new ArgumentNullException(nameof(fr8Account));
            }
            if (configRepository == null)
            {
                throw new ArgumentNullException(nameof(configRepository));
            }
            _activity = activity;
            _crateManager = crateManager;
            _plan = plan;
            _unitOfWorkFactory = unitOfWorkFactory;
            _fr8Account = fr8Account;
            _configRepository = configRepository;
        }
        /// <summary>
        /// Checks if there is already monitoring plan and activates it (if necessary). Otherwise creates a new plan and activates it
        /// </summary>
        public async Task<ManifestRegistryMonitorResult> StartMonitoringManifestRegistrySubmissions()
        {
            //If start attempt is already in progress we just reuse it and wait for the result
            if (Interlocked.CompareExchange(ref _isRunning, 1, 0) != 0)
            {
                Logger.GetLogger().Info($"{ManifestMonitoringPrefix}Plan creation is in progress, skipping the new attempt");
                return await _currentRun.Task;
            }
            //Otherwise we run a new attempt
            try
            {
                _currentRun = new TaskCompletionSource<ManifestRegistryMonitorResult>();
                Logger.GetLogger().Info($"{ManifestMonitoringPrefix}Retrieving system user");
                var systemUser = _fr8Account.GetSystemUser();
                if (systemUser == null)
                {
                    Logger.GetLogger().Error($"{ManifestMonitoringPrefix}System user doesn't exists");
                    throw new ApplicationException("System user doesn't exist");
                }
                var isNewPlanCreated = false;
                Logger.GetLogger().Info($"{ManifestMonitoringPrefix}Trying to find existing plan");
                var plan = GetExistingPlan(systemUser);
                if (plan == null)
                {
                    Logger.GetLogger().Info($"{ManifestMonitoringPrefix}No existing plan found. Creating new plan");
                    isNewPlanCreated = true;
                    plan = await CreateAndConfigureNewPlan(systemUser);
                    Logger.GetLogger().Info($"{ManifestMonitoringPrefix}New plan was created (Id - {plan.Id})");
                }
                else
                {
                    Logger.GetLogger().Info($"{ManifestMonitoringPrefix}Existing plan was found (Id - {plan.Id})");
                }
                try
                {
                    Logger.GetLogger().Info($"{ManifestMonitoringPrefix}Trying to launch the plan");
                    await RunPlan(plan);
                    Logger.GetLogger().Info($"{ManifestMonitoringPrefix}Plan was successfully launched");
                }
                catch (Exception ex)
                {
                    Logger.GetLogger().Error($"{ManifestMonitoringPrefix}Failed to launch a plan. {ex}");
                    if (isNewPlanCreated)
                    {
                        await _plan.Delete(plan.Id);
                    }
                    throw;
                }
                _currentRun.SetResult(new ManifestRegistryMonitorResult(plan.Id, isNewPlanCreated));
            }
            catch (Exception ex)
            {
                _currentRun.SetException(ex);
            }
            finally
            {
                Interlocked.Exchange(ref _isRunning, 0);
            }
            return await _currentRun.Task;
        }

        private async Task RunPlan(PlanDO plan)
        {
            if (plan.PlanState == PlanState.Executing || plan.PlanState == PlanState.Active)
            {
                return;
            }
            await _plan.Run(plan.Id, null, null);
        }

        private async Task<PlanDO> CreateAndConfigureNewPlan(Fr8AccountDO systemUser)
        {
            using (var uow = _unitOfWorkFactory.Create())
            {
                var activityTemplates = uow.ActivityTemplateRepository.GetQuery().ToArray();
                var result = await CreatePlanWithMonitoringActivity(uow, systemUser, activityTemplates).ConfigureAwait(false);
                Logger.GetLogger().Info($"{ManifestMonitoringPrefix}Created a plan");
                try
                {
                    await ConfigureMonitoringActivity(uow, systemUser, result.ChildNodes[0].ChildNodes[0] as ActivityDO).ConfigureAwait(false);
                    Logger.GetLogger().Info($"{ManifestMonitoringPrefix}Configured Monitor Form Response activity");
                    await ConfigureBuildMessageActivity(uow, systemUser, activityTemplates, result.Id).ConfigureAwait(false);
                    Logger.GetLogger().Info($"{ManifestMonitoringPrefix}Configured Build Message activity");
                    await ConfigureSaveJiraActivity(uow, systemUser, activityTemplates, result.Id).ConfigureAwait(false);
                    Logger.GetLogger().Info($"{ManifestMonitoringPrefix}Configured Save Jira activity");
                }
                catch
                {
                    await _plan.Delete(result.Id);
                    throw;
                }
                return result;
            }
        }

        private async Task ConfigureSaveJiraActivity(IUnitOfWork uow, Fr8AccountDO systemUser, ActivityTemplateDO[] activityTemplates, Guid planId)
        {
            var saveJiraTemplate = activityTemplates.FirstOrDefault(x => x.Name == "Save_Jira_Issue" && x.Version == "1" && x.Terminal.Name == "terminalAtlassian");
            if (saveJiraTemplate == null)
            {
                throw new ApplicationException("Save Jira Issue v1 activity template was not found in Atlassian terminal");
            }
            var saveJiraActivity = await _activity.CreateAndConfigure(uow, systemUser.Id, saveJiraTemplate.Id, saveJiraTemplate.Label, saveJiraTemplate.Label, 3, planId).ConfigureAwait(false) as ActivityDO;
            using (var storage = _crateManager.GetUpdatableStorage(saveJiraActivity))
            {
                var controls = storage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();
                if (controls == null)
                {
                    throw new ApplicationException("Save Jira Issue doesn't contain controls crate");
                }
                var projectSelector = controls.FindByName<DropDownList>("AvailableProjects");
                if (projectSelector == null)
                {
                    throw new ApplicationException("Save Jira Issue doesn't contain project selector");
                }
                projectSelector.SelectByKey("fr8test");
            }
            saveJiraActivity = Mapper.Map<ActivityDO>(await _activity.Configure(uow, systemUser.Id, saveJiraActivity).ConfigureAwait(false));
            using (var storage = _crateManager.GetUpdatableStorage(saveJiraActivity))
            {
                var controls = storage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();
                var issueTypeSelector = controls.FindByName<DropDownList>("AvailableIssueTypes");
                if (issueTypeSelector == null)
                {
                    throw new ApplicationException("Save Jira Issue doesn't contain issue type selector");
                }
                issueTypeSelector.SelectByKey("Task");
            }
            saveJiraActivity = Mapper.Map<ActivityDO>(await _activity.Configure(uow, systemUser.Id, saveJiraActivity).ConfigureAwait(false));
            using (var storage = _crateManager.GetUpdatableStorage(saveJiraActivity))
            {
                var controls = storage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();
                var prioritySelector = controls.FindByName<DropDownList>("AvailablePriorities");
                if (prioritySelector == null)
                {
                    throw new ApplicationException("Save Jira Issue doesn't contain priority selector");
                }
                prioritySelector.SelectByKey("Normal");
                var assigneeSelector = controls.FindByName<DropDownList>("Asignee");
                if (assigneeSelector == null)
                {
                    throw new ApplicationException("Save Jira Issue doesn't contain asignee selector");
                }
                assigneeSelector.SelectByValue("admin");
                var summary = controls.FindByName<TextSource>("SummaryTextSource");
                if (summary == null)
                {
                    throw new ApplicationException("Save Jira Issue doesn't contain summary field");
                }
                summary.ValueSource = TextSource.SpecificValueSource;
                summary.TextValue = "New Manifest Submission";
                var description = controls.FindByName<TextSource>("DescriptionTextSource");
                if (description == null)
                {
                    throw new ApplicationException("Save Jira Issue doesn't contain description field");
                }
                description.ValueSource = TextSource.UpstreamValueSrouce;
                description.SelectedItem = new FieldDTO(MessageName);
                description.selectedKey = MessageName;

            }
            await _activity.Configure(uow, systemUser.Id, saveJiraActivity).ConfigureAwait(false);
        }

        private async Task ConfigureBuildMessageActivity(IUnitOfWork uow, Fr8AccountDO systemUser, ActivityTemplateDO[] activityTemplates, Guid planId)
        {
            var buildMessageTemplate = activityTemplates.FirstOrDefault(x => x.Name == "Build_Message" && x.Version == "1" && x.Terminal.Name == "terminalFr8Core");
            if (buildMessageTemplate == null)
            {
                throw new ApplicationException("Build Message v1 activity template was not found in Fr8Core terminal");
            }
            var buildMessageActivity = await _activity.CreateAndConfigure(uow, systemUser.Id, buildMessageTemplate.Id, buildMessageTemplate.Label, buildMessageTemplate.Label, 2, planId).ConfigureAwait(false) as ActivityDO;
            using (var storage = _crateManager.GetUpdatableStorage(buildMessageActivity))
            {
                var controls = storage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();
                if (controls == null)
                {
                    throw new ApplicationException("Build Message doesn't contain controls crate");
                }
                var messageName = controls.FindByName<TextBox>("Name");
                if (messageName == null)
                {
                    throw new ApplicationException("Build Message doesn't contain message name control");
                }
                messageName.Value = MessageName;
                var messageBody = controls.FindByName<BuildMessageAppender>("Body");
                if (messageBody == null)
                {
                    throw new ApplicationException("Build Message doesn't contain message body control");
                }
                messageBody.Value =
@"*Manifest Type:*
[Manifest Type Name]
*Version:*
[Version]
*Sample JSON:*
[Sample JSON]
*Description:*
[Description]
*Submitter Name:*
[Your Name]
*Submitter Email:*
[Your Email]";
            }
            await _activity.Configure(uow, systemUser.Id, buildMessageActivity).ConfigureAwait(false);
        }

        private async Task ConfigureMonitoringActivity(IUnitOfWork uow, Fr8AccountDO systemUser, ActivityDO monitoringActivity)
        {
            using (var activityStorage = _crateManager.GetUpdatableStorage(monitoringActivity))
            {
                var authenticationCrate = activityStorage.FirstCrateOrDefault<StandardAuthenticationCM>();
                if (authenticationCrate != null)
                {
                    throw new ApplicationException("There is no default authentication token for system user in Google terminal");
                }
                var controls = activityStorage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();
                if (controls == null)
                {
                    throw new ApplicationException("Monitor Form Responses doesn't contain controls crate");
                }
                var formsSelector = controls.FindByName<DropDownList>("Selected_Google_Form");
                if (formsSelector == null)
                {
                    throw new ApplicationException("Monitor Form Responses doesn't contain form selector control");
                }
                var formId = _configRepository.Get("ManifestSubmissionFormId");
                if (string.IsNullOrEmpty(formId))
                {
                    throw new ApplicationException("Configuration doesn't contain info about form Id to monitor");
                }
                formsSelector.SelectByValue(formId);
                if (string.IsNullOrEmpty(formsSelector.Value))
                {
                    throw new ApplicationException("Form specified in configuration doesn't belong to system user's account Google authorization");
                }
            }
            await _activity.Configure(uow, systemUser.Id, monitoringActivity).ConfigureAwait(false);
        }

        private async Task<PlanDO> CreatePlanWithMonitoringActivity(IUnitOfWork uow, Fr8AccountDO systemUser, ActivityTemplateDO[] activityTemplates)
        {
            var monitorFormResponseTemplate = activityTemplates.FirstOrDefault(x => x.Name == "Monitor_Form_Responses" && x.Version == "1" && x.Terminal.Name == "terminalGoogle");
            if (monitorFormResponseTemplate == null)
            {
                throw new ApplicationException("Monitor Form Responses v1 activity template was not found in Google terminal");
            }
            return await _activity.CreateAndConfigure(
                                    uow,
                                    systemUser.Id, 
                                    monitorFormResponseTemplate.Id, 
                                    monitorFormResponseTemplate.Label, 
                                    MonitoringPlanName, 
                                    1, 
                                    createPlan: true)
                                  .ConfigureAwait(false) as PlanDO;
        }

        private PlanDO GetExistingPlan(Fr8AccountDO systemUser)
        {
            using (var uow = _unitOfWorkFactory.Create())
            {
                return uow.PlanRepository.GetPlanQueryUncached().FirstOrDefault(x => x.Name == MonitoringPlanName
                                                                                     && x.Fr8AccountId == systemUser.Id
                                                                                     && x.PlanState != PlanState.Deleted);
            }
        }
    }
}

