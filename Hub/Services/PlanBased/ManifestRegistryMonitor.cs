using System;
using System.Data.Entity;
using System.Linq;
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
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities;
using Hub.Interfaces;
using Hub.Managers;

namespace Hub.Services
{
    public class ManifestRegistryMonitor : IManifestRegistryMonitor
    {
        private readonly IActivity _activity;

        private readonly ICrateManager _crateManager;

        private readonly IPlan _plan;

        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        private readonly IConfigRepository _configRepository;

        private const string MonitoringPlanName = "Monitoring Manifest Submissions";

        private const string MessageName = "JIRA description";

        public ManifestRegistryMonitor(
            IActivity activity,
            ICrateManager crateManager,
            IPlan plan, 
            IUnitOfWorkProvider unitOfWorkProvider, 
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
            if (unitOfWorkProvider == null)
            {
                throw new ArgumentNullException(nameof(unitOfWorkProvider));
            }
            if (configRepository == null)
            {
                throw new ArgumentNullException(nameof(configRepository));
            }
            _activity = activity;
            _crateManager = crateManager;
            _plan = plan;
            _unitOfWorkProvider = unitOfWorkProvider;
            _configRepository = configRepository;
        }

        public async Task<ManifestRegistryMonitorResult> StartMonitoringManifestRegistrySubmissions()
        {
            var systemUser = GetSystemUser();
            if (systemUser == null)
            {
                throw new ApplicationException("System user doesn't exist");
            }
            var isNewPlanCreated = false;
            var plan = GetExistingPlan(systemUser);
            if (plan == null)
            {
                isNewPlanCreated = true;
                plan = await CreateAndConfigureNewPlan(systemUser);
            }
            try
            {
                await RunPlan(plan);
            }
            catch
            {
                if (isNewPlanCreated)
                {
                    await _plan.Delete(plan.Id);
                }
                throw;
            }
            return new ManifestRegistryMonitorResult(plan.Id, isNewPlanCreated);
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
                var activityTemplates = uow.ActivityTemplateRepository.GetQuery().ToArray();
                var result = await CreatePlanWithMonitoringActivity(uow, systemUser, activityTemplates).ConfigureAwait(false);
                try
                {
                    await ConfigureMonitoringActivity(uow, systemUser, result.ChildNodes[0].ChildNodes[0] as ActivityDO).ConfigureAwait(false);
                    await ConfigureBuildMessageActivity(uow, systemUser, activityTemplates, result.Id).ConfigureAwait(false);
                    await ConfigureSaveJiraActivity(uow, systemUser, activityTemplates, result.Id).ConfigureAwait(false);
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
                projectSelector.SelectByKey("Fr8");
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
            return await _activity.CreateAndConfigure(uow, systemUser.Id, monitorFormResponseTemplate.Id, monitorFormResponseTemplate.Label, MonitoringPlanName, 1, createPlan: true)
                                  .ConfigureAwait(false)as PlanDO;
        }

        private PlanDO GetExistingPlan(Fr8AccountDO systemUser)
        {
            using (var uow = _unitOfWorkProvider.GetNewUnitOfWork())
            {
                return uow.PlanRepository.GetPlanQueryUncached().FirstOrDefault(x => x.Name == MonitoringPlanName
                                                                                           && x.Fr8AccountId == systemUser.Id
                                                                                           && x.Visibility == PlanVisibility.Internal
                                                                                           && x.PlanState != PlanState.Deleted);
            }
        }

        private Fr8AccountDO GetSystemUser()
        {
            try
            {
                var systemUserEmail = _configRepository.Get("SystemUserEmail");
                using (var uow = _unitOfWorkProvider.GetNewUnitOfWork())
                {
                    return uow.UserRepository.GetQuery().FirstOrDefault(x => x.EmailAddress.Address == systemUserEmail);
                }
            }
            catch (ConfigurationException)
            {
                throw new ApplicationException("Configuration repository doesn't contain system user email");
            }
        }
    }
}
