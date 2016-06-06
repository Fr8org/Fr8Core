using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using Fr8Data.States;
using Hub.Interfaces;
using Utilities;
using Utilities.Logging;

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

        public async Task StartMonitoringManifestRegistrySubmissions()
        {
            var systemUser = await GetSystemUser();
            if (systemUser == null)
            {
                throw new ApplicationException("System user doesn't exist");
            }
            var plan = await GetExistingPlan(systemUser) ?? await CreateAndConfigureNewPlan(systemUser);
        }

        private async Task<PlanDO> CreateAndConfigureNewPlan(Fr8AccountDO systemUser)
        {
            using (var uow = _unitOfWorkProvider.GetNewUnitOfWork())
            {
                var activityTemplates = await uow.ActivityTemplateRepository.GetQuery().ToArrayAsync();
                var result = await CreatePlanWithMonitoringActivity(uow, systemUser, activityTemplates);
                var monitoringActivity = result.ChildNodes[0].ChildNodes[0] as ActivityDO;
                result.ChildNodes[0] = await ConfigureMonitoringActivity(monitoringActivity);
                return result;
            }
        }

        private async Task<PlanNodeDO> ConfigureMonitoringActivity(ActivityDO monitoringActivity)
        {
            var activityStorage = _crateManager.GetStorage(monitoringActivity.CrateStorage);
            var authenticationCrate = activityStorage.FirstCrateOrDefault<StandardAuthenticationCM>();
            if (authenticationCrate != null)
            {
                throw new ApplicationException("There is no default authentication token for system user in Google terminal");
            }
            var controls = activityStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();
            if (controls == null)
            {
                throw new ApplicationException("Monitor Form Responses doesn't contain controls crate");
            }
            var formsSelector = controls.FindByName<DropDownList>("Selected_Google_Form");
            if (formsSelector == null)
            {
                throw new ApplicationException("Monitor Form Responses doesn't contain form selector control");
            }

            //TODO: 
            return monitoringActivity;
        }

        private async Task<PlanDO> CreatePlanWithMonitoringActivity(IUnitOfWork uow, Fr8AccountDO systemUser, ActivityTemplateDO[] activityTemplates)
        {
            var monitorFormResponseTemplate = activityTemplates.FirstOrDefault(x => x.Name == "Monitor_Form_Responses" && x.Version == "1" && x.Terminal.Name == "terminalGoogle");
            if (monitorFormResponseTemplate == null)
            {
                throw new ApplicationException("Monitor Form Responses v1 activity template was not found in Google terminal");
            }
            return await _activity.CreateAndConfigure(uow, systemUser.Id, monitorFormResponseTemplate.Id, monitorFormResponseTemplate.Label, monitorFormResponseTemplate.Label, 1, createPlan: true) as PlanDO;
        }

        private async Task<PlanDO> GetExistingPlan(Fr8AccountDO systemUser)
        {
            using (var uow = _unitOfWorkProvider.GetNewUnitOfWork())
            {
                return await uow.PlanRepository.GetPlanQueryUncached().FirstOrDefaultAsync(x => x.Name == MonitoringPlanName
                                                                                                && x.Visibility == PlanVisibility.Internal
                                                                                                && x.Fr8AccountId == systemUser.Id
                                                                                                && x.PlanState != 3); //Not deleted
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
