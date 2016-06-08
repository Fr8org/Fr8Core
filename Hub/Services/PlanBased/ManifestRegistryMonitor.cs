using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using Fr8Data.States;
using Hub.Interfaces;
using Hub.Managers;
using Utilities;

namespace Hub.Services
{
    public class ManifestRegistryMonitor : IManifestRegistryMonitor
    {
        private readonly IActivity _activity;

        private readonly IActivityTemplate _activityTemplate;

        private readonly ICrateManager _crateManager;

        private readonly IPlan _plan;

        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        private readonly IConfigRepository _configRepository;

        private const string MonitoringPlanName = "Monitoring Manifest Submissions";

        private const string MessageName = "JIRA description";

        public ManifestRegistryMonitor(
            IActivity activity, 
            IActivityTemplate activityTemplate,
            ICrateManager crateManager,
            IPlan plan, 
            IUnitOfWorkProvider unitOfWorkProvider, 
            IConfigRepository configRepository)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }
            if (activityTemplate == null)
            {
                throw new ArgumentNullException(nameof(activityTemplate));
            }
            if (crateManager == null)
            {
                throw new ArgumentNullException(nameof(crateManager));
            }
            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }
            if (unitOfWorkProvider == null)
            {
                throw new ArgumentNullException(nameof(unitOfWorkProvider));
            }
            if (configRepository == null)
            {
                throw new ArgumentNullException(nameof(configRepository));
            }
            _activity = activity;
            _activityTemplate = activityTemplate;
            _crateManager = crateManager;
            _plan = plan;
            _unitOfWorkProvider = unitOfWorkProvider;
            _configRepository = configRepository;
        }

        public async Task<bool> StartMonitoringManifestRegistrySubmissions()
        {
            var systemUser = await GetSystemUser();
            if (systemUser == null)
            {
                throw new ApplicationException("System user doesn't exist");
            }
            var isNewPlanCreated = false;
            var plan = await GetExistingPlan();
            if (plan == null)
            {
                isNewPlanCreated = true;
                plan = await CreateAndConfigureNewPlan(systemUser);
            }
            await RunPlan(plan);
            return isNewPlanCreated;
        }

        private async Task RunPlan(PlanDO plan)
        {
            if (plan.PlanState == PlanState.Running)
            {
                return;
            }
            await _plan.Run(plan.Id, new[] { Crate<OperationalStateCM>.FromContent(string.Empty, new OperationalStateCM()) }, null);
        }

        private async Task<PlanDO> CreateAndConfigureNewPlan(Fr8AccountDO systemUser)
        {
            using (var uow = _unitOfWorkProvider.GetNewUnitOfWork())
            {
                var activityTemplates = await uow.ActivityTemplateRepository.GetQuery().ToArrayAsync();
                var result = await CreatePlanWithMonitoringActivity(uow, systemUser, activityTemplates);
                await ConfigureMonitoringActivity(uow, systemUser, result.ChildNodes[0].ChildNodes[0] as ActivityDO);
                await ConfigureBuildMessageActivity(uow, systemUser, activityTemplates, result.Id);
                await ConfigureSaveJiraActivity(uow, systemUser, activityTemplates, result.Id);
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
            var saveJiraActivity = await _activity.CreateAndConfigure(uow, systemUser.Id, saveJiraTemplate.Id, saveJiraTemplate.Label, saveJiraTemplate.Label, 3, planId) as ActivityDO;
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
                projectSelector.SelectByKey("Fr8");
            }
            saveJiraActivity = Mapper.Map<ActivityDO>(await _activity.Configure(uow, systemUser.Id, saveJiraActivity));
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
            saveJiraActivity = Mapper.Map<ActivityDO>(await _activity.Configure(uow, systemUser.Id, saveJiraActivity));
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
            await _activity.Configure(uow, systemUser.Id, saveJiraActivity);
        }

        private async Task ConfigureBuildMessageActivity(IUnitOfWork uow, Fr8AccountDO systemUser, ActivityTemplateDO[] activityTemplates, Guid planId)
        {
            var buildMessageTemplate = activityTemplates.FirstOrDefault(x => x.Name == "Build_Message" && x.Version == "1" && x.Terminal.Name == "terminalFr8Core");
            if (buildMessageTemplate == null)
            {
                throw new ApplicationException("Build Message v1 activity template was not found in Fr8Core terminal");
            }
            var buildMessageActivity = await _activity.CreateAndConfigure(uow, systemUser.Id, buildMessageTemplate.Id, buildMessageTemplate.Label, buildMessageTemplate.Label, 2, planId) as ActivityDO;
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
            await _activity.Configure(uow, systemUser.Id, buildMessageActivity);
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
            await _activity.Configure(uow, systemUser.Id, monitoringActivity);
        }

        private async Task<PlanDO> CreatePlanWithMonitoringActivity(IUnitOfWork uow, Fr8AccountDO systemUser, ActivityTemplateDO[] activityTemplates)
        {
            var monitorFormResponseTemplate = activityTemplates.FirstOrDefault(x => x.Name == "Monitor_Form_Responses" && x.Version == "1" && x.Terminal.Name == "terminalGoogle");
            if (monitorFormResponseTemplate == null)
            {
                throw new ApplicationException("Monitor Form Responses v1 activity template was not found in Google terminal");
            }
            return await _activity.CreateAndConfigure(uow, systemUser.Id, monitorFormResponseTemplate.Id, monitorFormResponseTemplate.Label, MonitoringPlanName, 1, createPlan: true, isInternalPlan: true) as PlanDO;
        }

        private async Task<PlanDO> GetExistingPlan()
        {
            using (var uow = _unitOfWorkProvider.GetNewUnitOfWork())
            {
                return await uow.PlanRepository.GetPlanQueryUncached().FirstOrDefaultAsync(x => x.Name == MonitoringPlanName
                                                                                                && x.Visibility == PlanVisibility.Internal
                                                                                                && x.PlanState != PlanState.Deleted);
            }
        }

        private async Task<Fr8AccountDO> GetSystemUser()
        {
            var systemUserEmail = _configRepository.Get("SystemUserEmail");
            if (string.IsNullOrEmpty(systemUserEmail))
            {
                throw new ApplicationException("Configuration repository doesn't contain system user email");
            }
            using (var uow = _unitOfWorkProvider.GetNewUnitOfWork())
            {
                return await uow.UserRepository.GetQuery().FirstOrDefaultAsync(x => x.EmailAddress.Address == systemUserEmail);
            }
        }
    }
}
