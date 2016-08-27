using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AutoMapper;
using StructureMap;
using Data.Entities;
using Data.Exceptions;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.States;
using Hub.Interfaces;
using InternalInterface = Hub.Interfaces;
using System.Threading.Tasks;
using Data.Infrastructure;
using Data.Repositories.Plan;
using Fr8.Infrastructure;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.DataTransferObjects.Helpers;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Logging;
using Hub.Exceptions;
using Fr8.Infrastructure.Utilities;
using Hub.Managers.APIManagers.Packagers;

namespace Hub.Services
{
    public class Plan : IPlan
    {
        private const int DefaultPlanPageSize = 10;
        private const int MinPlanPageSize = 5;

        private readonly InternalInterface.IContainerService _containerService;
        private readonly Fr8Account _dockyardAccount;
        private readonly IActivity _activity;
        private readonly ICrateManager _crate;
        private readonly ISecurityServices _security;
        private readonly IJobDispatcher _dispatcher;
        private readonly IPusherNotifier _pusherNotifier;

        public Plan(IContainerService containerService, Fr8Account dockyardAccount, IActivity activity,
            ICrateManager crate, ISecurityServices security, IJobDispatcher dispatcher, IPusherNotifier pusher)
        {
            _containerService = containerService;
            _dockyardAccount = dockyardAccount;
            _activity = activity;
            _crate = crate;
            _security = security;
            _dispatcher = dispatcher;
            _pusherNotifier = pusher;
        }

        public PlanResultDTO GetForUser(IUnitOfWork unitOfWork, Fr8AccountDO account, PlanQueryDTO planQueryDTO, bool isAdmin = false)
        {
            //lets make sure our inputs are correct
            planQueryDTO = planQueryDTO ?? new PlanQueryDTO();
            planQueryDTO.Page = planQueryDTO.Page ?? 1;
            planQueryDTO.Page = planQueryDTO.Page < 1 ? 1 : planQueryDTO.Page;
            planQueryDTO.PlanPerPage = planQueryDTO.PlanPerPage ?? DefaultPlanPageSize;
            planQueryDTO.PlanPerPage = planQueryDTO.PlanPerPage < MinPlanPageSize ? MinPlanPageSize : planQueryDTO.PlanPerPage;
            planQueryDTO.IsDescending = planQueryDTO.IsDescending ?? planQueryDTO.OrderBy?.StartsWith("-") ?? true;
            if (planQueryDTO.OrderBy?.StartsWith("-") == true)
            {
                planQueryDTO.OrderBy = planQueryDTO.OrderBy.Substring(1);
            }

            var planQuery = unitOfWork.PlanRepository.GetPlanQueryUncached()
                .Where(x => x.Visibility == PlanVisibility.Standard);

            if (planQueryDTO.AppsOnly)
            {
                planQuery = planQueryDTO.Id == null
                    ? planQuery.Where(pt => pt.IsApp == true)
                    : planQuery.Where(pt => pt.Id == planQueryDTO.Id && pt.IsApp == true);

                if (account.OrganizationId.HasValue)
                {
                    // If the current user belongs to some organization, 
                    // display all apps for that organization
                    planQuery = planQuery.Where(pt => pt.Fr8Account.OrganizationId == account.OrganizationId);
                }
                else
                {
                    // If user does not belong to an org, just display his/her own apps.
                    planQuery = planQuery.Where(pt => pt.Fr8Account.Id == account.Id);
                }
            }
            else
            {
                planQuery = planQueryDTO.Id == null
                    ? planQuery.Where(pt => pt.Fr8Account.Id == account.Id)
                    : planQuery.Where(pt => pt.Id == planQueryDTO.Id && pt.Fr8Account.Id == account.Id);
            }


            planQuery = !string.IsNullOrEmpty(planQueryDTO.Category)
                ? planQuery.Where(c => c.Category == planQueryDTO.Category)
                : planQuery.Where(c => string.IsNullOrEmpty(c.Category));

            if (!string.IsNullOrEmpty(planQueryDTO.Filter))
            {
                planQuery = planQuery.Where(c => c.Name.Contains(planQueryDTO.Filter) || c.Description.Contains(planQueryDTO.Filter));
            }

            if (planQueryDTO.AppsOnly)
            {
                planQuery = planQuery.Where(c => c.IsApp == true);
            }

            int? planState = null;

            if (planQueryDTO.Status != null)
            {
                planState = PlanState.StringToInt(planQueryDTO.Status);
            }

            planQuery = planState == null
                ? planQuery.Where(pt => pt.PlanState != PlanState.Deleted)
                : planQuery.Where(pt => pt.PlanState == planState);

            // Lets allow ordering with just name for now
            if (planQueryDTO.OrderBy == "name")
            {
                planQuery = planQueryDTO.IsDescending.Value
                    ? planQuery.OrderByDescending(p => p.Name)
                    : planQuery.OrderBy(p => p.Name);
            }
            else
            {
                planQuery = planQueryDTO.IsDescending.Value
                    ? planQuery.OrderByDescending(p => p.LastUpdated)
                    : planQuery.OrderBy(p => p.LastUpdated);
            }

            var totalPlanCountForCurrentCriterias = planQuery.Count();

            planQuery = planQuery.Skip(planQueryDTO.PlanPerPage.Value * (planQueryDTO.Page.Value - 1))
                    .Take(planQueryDTO.PlanPerPage.Value);

            return new PlanResultDTO
            {
                Plans = planQuery.ToList().Select(Mapper.Map<PlanNoChildrenDTO>).ToList(),
                CurrentPage = planQueryDTO.Page.Value,
                TotalPlanCount = totalPlanCountForCurrentCriterias
            };

        }

        public int UserPlansCount(IUnitOfWork uow, string userId)
        {
            return uow.PlanRepository.GetPlanQueryUncached().Where(p => p.Fr8AccountId == userId && p.Visibility == PlanVisibility.Standard).Count();
        }

        public int? GetPlanState(IUnitOfWork uow, Guid planNodeId)
        {
            var existingNode = uow.PlanRepository.GetById<PlanNodeDO>(planNodeId);
            var root = existingNode?.GetTreeRoot() as PlanDO;

            return root?.PlanState;
        }

        public bool IsMonitoringPlan(IUnitOfWork uow, PlanDO plan)
        {
            var solutionId = ActivityCategories.SolutionId;
            var monitorId = ActivityCategories.MonitorId;

            var initialActivity = plan.StartingSubplan.GetDescendantsOrdered()
                .OfType<ActivityDO>()
                .FirstOrDefault(x => !uow.ActivityTemplateRepository.GetByKey(x.ActivityTemplateId).Categories.Any(y => y.ActivityCategoryId == solutionId));

            if (initialActivity == null)
            {
                return false;
            }

            var activityTemplate = uow.ActivityTemplateRepository.GetByKey(initialActivity.ActivityTemplateId);

            if (activityTemplate.Categories.Any(y => y.ActivityCategoryId == monitorId))
            {
                return true;
            }

            var storage = _crate.GetStorage(initialActivity.CrateStorage);

            // First activity has event subsribtions. This means that this plan can be triggered by external event
            if (storage.CrateContentsOfType<EventSubscriptionCM>().Any(x => x.Subscriptions?.Count > 0))
            {
                return true;
            }

            return false;
        }

        public IList<PlanDO> GetByName(IUnitOfWork uow, Fr8AccountDO account, string name, PlanVisibility visibility)
        {
            if (name != null)
            {
                return
                    uow.PlanRepository.GetPlanQueryUncached()
                        .Where(r => r.Fr8Account.Id == account.Id && r.Name == name)
                        .Where(p => p.PlanState != PlanState.Deleted && p.Visibility == visibility)
                        .ToList();
            }

            return
                uow.PlanRepository.GetPlanQueryUncached()
                    .Where(r => r.Fr8Account.Id == account.Id)
                    .Where(p => p.PlanState != PlanState.Deleted && p.Visibility == visibility)
                    .ToList();
        }

        public void CreateOrUpdate(IUnitOfWork uow, PlanDO submittedPlan)
        {
            if (submittedPlan.Id == Guid.Empty)
            {
                submittedPlan.Id = Guid.NewGuid();
                submittedPlan.PlanState = PlanState.Inactive;
                submittedPlan.Fr8Account = _security.GetCurrentAccount(uow);
                if (string.IsNullOrEmpty(submittedPlan.Name))
                {
                    submittedPlan.Name = "Untitled Plan " + (UserPlansCount(uow, _security.GetCurrentUser()) + 1);
                }

                submittedPlan.ChildNodes.Add(new SubplanDO(true)
                {
                    Id = Guid.NewGuid(),
                    Name = "Starting Subplan"
                });

                uow.PlanRepository.Add(submittedPlan);
            }
            else
            {
                var curPlan = uow.PlanRepository.GetById<PlanDO>(submittedPlan.Id);
                if (curPlan == null)
                {
                    throw new EntityNotFoundException();
                }

                curPlan.Name = submittedPlan.Name;
                curPlan.Description = submittedPlan.Description;
                curPlan.Category = submittedPlan.Category;
                curPlan.LastUpdated = DateTimeOffset.UtcNow;
                curPlan.IsApp = submittedPlan.IsApp;
                curPlan.AppLaunchURL = submittedPlan.AppLaunchURL;
            }
        }

        public PlanDO GetFullPlan(IUnitOfWork uow, Guid id)
        {
            return uow.PlanRepository.GetById<PlanDO>(id);
        }

        public PlanDO Create(IUnitOfWork uow, string name, string category = "", string ownerId = "", PlanVisibility visibility = PlanVisibility.Standard)
        {
            var plan = new PlanDO
            {
                Id = Guid.NewGuid(),
                Name = name,
                Fr8Account = string.IsNullOrEmpty(ownerId) ? _security.GetCurrentAccount(uow) : uow.UserRepository.FindOne(x => x.Id == ownerId),
                PlanState = PlanState.Inactive,
                Visibility = visibility,
                Category = category
            };

            uow.PlanRepository.Add(plan);
            return plan;
        }

        public async Task Delete(Guid id)
        {
            Exception deactivationFailure = null;

            try
            {
                await Deactivate(id);
            }
            catch (Exception ex)
            {
                deactivationFailure = ex;
            }

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = uow.PlanRepository.GetById<PlanDO>(id);

                if (plan == null)
                {
                    throw new EntityNotFoundException<PlanDO>(id);
                }

                //Plan deletion will only update its PlanState = Deleted
                foreach (var container in _containerService.LoadContainers(uow, plan))
                {
                    container.State = State.Deleted;
                }

                if (deactivationFailure == null)
                {
                    plan.PlanState = PlanState.Deleted;
                }

                uow.SaveChanges();
            }

            if (deactivationFailure != null)
            {
                throw deactivationFailure;
            }
        }

        public async Task<ActivateActivitiesDTO> Activate(Guid curPlanId, bool planBuilderActivate)
        {
            var result = new ActivateActivitiesDTO();

            List<Task<ActivityDTO>> activitiesTask = new List<Task<ActivityDTO>>();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = uow.PlanRepository.GetById<PlanDO>(curPlanId);

                if (plan.PlanState == PlanState.Deleted)
                {
                    EventManager.PlanActivationFailed(plan, "Cannot activate deleted plan");
                    throw new ApplicationException("Cannot activate deleted plan");
                }

                try
                {
                    foreach (var curActionDO in plan.GetDescendants().OfType<ActivityDO>())
                    {
                        activitiesTask.Add(_activity.Activate(curActionDO));
                    }

                    await Task.WhenAll(activitiesTask);
                }
                catch (AggregateException ex)
                {
                    foreach (Exception e in ex.InnerExceptions)
                    {
                        if (e is InvalidTokenRuntimeException)
                        {
                            ReportAuthError(uow, plan.Fr8Account, (InvalidTokenRuntimeException)e);
                        }
                    }

                    EventManager.PlanActivationFailed(plan, "Unable to activate plan");
                    throw;
                }
                catch (InvalidTokenRuntimeException ex)
                {
                    ReportAuthError(uow, plan.Fr8Account, (InvalidTokenRuntimeException)ex);
                    EventManager.PlanActivationFailed(plan, "Unable to activate plan");
                    throw;
                }

                foreach (var resultActivate in activitiesTask.Select(x => x.Result))
                {
                    var errors = new ValidationErrorsDTO(ExtractValidationErrors(resultActivate));

                    if (errors.ValidationErrors.Count > 0)
                    {
                        result.ValidationErrors[resultActivate.Id] = errors;
                    }
                }

                if (result.ValidationErrors.Count == 0 && plan.PlanState != PlanState.Executing)
                {
                    plan.PlanState = IsMonitoringPlan(uow, plan) ? PlanState.Active : PlanState.Executing;
                    plan.LastUpdated = DateTimeOffset.UtcNow;
                    uow.SaveChanges();
                }
            }

            return result;
        }

        /// <summary>
        /// After receiving response from terminals for activate action call, checks for existing validation errors on some controls
        /// </summary>
        /// <param name="curActivityDTO"></param>
        /// <param name="containerDTO">Use containerDTO as a wrapper for the Error with proper ActivityResponse and error DTO</param>
        /// <returns></returns>
        public IEnumerable<ValidationResultDTO> ExtractValidationErrors(ActivityDTO curActivityDTO)
        {
            var crateStorage = _crate.GetStorage(curActivityDTO);
            return crateStorage.CrateContentsOfType<ValidationResultsCM>().SelectMany(x => x.ValidationErrors);
        }

        public bool IsPlanActiveOrExecuting(Guid planNodeId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var planState = GetPlanState(uow, planNodeId);
                if (planState == PlanState.Executing || planState == PlanState.Active)
                {
                    return true;
                }
                return false;
            }
        }

        public async Task Deactivate(Guid planId)
        {
            var deactivateTasks = new List<Task>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = uow.PlanRepository.GetById<PlanDO>(planId);
                if (plan == null)
                {
                    throw new MissingObjectException($"Plan with Id {planId} doesn't exist");
                }
                plan.PlanState = PlanState.Inactive;
                uow.SaveChanges();

                foreach (var activity in plan.GetDescendants().OfType<ActivityDO>())
                {
                    deactivateTasks.Add(_activity.Deactivate(activity));
                }

                _pusherNotifier.NotifyUser(new NotificationMessageDTO
                {
                    NotificationType = NotificationType.ExecutionStopped,
                    Subject = "Plan Stopped",
                    Message = $"\"{plan.Name}\" has been stopped.",
                    Collapsed = false
                }, plan.Fr8AccountId);
            }

            try
            {
                await Task.WhenAll(deactivateTasks);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to deactivate plan", ex);
            }

            EventManager.PlanDeactivated(planId);
        }

        public IList<PlanDO> GetMatchingPlans(string userId, EventReportCM curEventReport)
        {
            if (String.IsNullOrEmpty(userId))
                throw new ArgumentNullException("Parameter UserId is null");

            if (curEventReport == null)
                throw new ArgumentNullException("Parameter Standard Event Report is null");

            //1. Query all PlanDO that are Active
            //2. are associated with the determined DockyardAccount
            //3. their first Activity has a Crate of  Class "Standard Event Subscriptions" which has inside of it an event name that matches the event name 
            //in the Crate of Class "Standard Event Reports" which was passed in.
            var curPlans = _dockyardAccount.GetActivePlans(userId).ToList();

            return MatchEvents(curPlans, curEventReport);
            //3. Get ActivityDO
        }

        public List<PlanDO> MatchEvents(List<PlanDO> curPlans, EventReportCM curEventReport)
        {
            List<PlanDO> subscribingPlans = new List<PlanDO>();
            foreach (var curPlan in curPlans)
            {
                // Get the first activity
                var actionDO = GetFirstActivityWithEventSubscriptions(curPlan.Id);

                if (actionDO != null)
                {
                    var storage = _crate.GetStorage(actionDO.CrateStorage);

                    foreach (var subscriptionsList in storage.CrateContentsOfType<EventSubscriptionCM>())
                    {
                        var manufacturer = subscriptionsList.Manufacturer;
                        bool hasEvents;
                        if (string.IsNullOrEmpty(manufacturer) || string.IsNullOrEmpty(curEventReport.Manufacturer))
                        {
                            hasEvents = subscriptionsList.Subscriptions.Any(events => curEventReport.EventNames.ToUpper().Trim().Replace(" ", "").Contains(events.ToUpper().Trim().Replace(" ", "")));
                        }
                        else
                        {
                            hasEvents = subscriptionsList.Subscriptions.Any(events => curEventReport.Manufacturer == manufacturer &&
                                curEventReport.EventNames.ToUpper().Trim().Replace(" ", "").Contains(events.ToUpper().Trim().Replace(" ", "")));
                        }

                        if (hasEvents)
                        {
                            subscribingPlans.Add(curPlan);
                        }
                    }
                }
            }
            return subscribingPlans;
        }

        private ActivityDO GetFirstActivityWithEventSubscriptions(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var root = uow.PlanRepository.GetById<PlanDO>(id);
                if (root == null)
                {
                    return null;
                }

                return root.GetDescendantsOrdered().OfType<ActivityDO>().FirstOrDefault(
                    x =>
                    {
                        var storage = _crate.GetStorage(x.CrateStorage);
                        return storage.CratesOfType<EventSubscriptionCM>().Any();
                    });
            }
        }

        public PlanDO GetPlanByActivityId(IUnitOfWork uow, Guid id)
        {
            return (PlanDO)uow.PlanRepository.GetById<PlanNodeDO>(id).GetTreeRoot();
        }

        public void Enqueue(Guid curPlanId, params Crate[] curEventReport)
        {
            // We convert incoming data to DTO objects because HangFire will serialize method parameters into JSON and serializing of Crate objects is forbidden
            var curEventReportDTO = curEventReport.Select(x => CrateStorageSerializer.Default.ConvertToDto(x)).ToArray();
            // We don't await this call as it will be awaited inside HangFire after job is launched
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            _dispatcher.Enqueue(() => LaunchPlanCallback(curPlanId, curEventReportDTO));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public static async Task LaunchPlanCallback(Guid planId, params CrateDTO[] curPayload)
        {
            Logger.GetLogger().Info($"Starting executing plan {planId} as a reaction to external event");

            if (planId == default(Guid))
            {
                Logger.GetLogger().Error("Can't lanunch plan with empty id");
            }

            // We "eat" this exception to make Hangfire thinks that everthying is good and job is completed
            // This exception should be already logged somewhere
            var planService = ObjectFactory.GetInstance<Plan>();

            try
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var plan = uow.PlanRepository.GetById<PlanDO>(planId);

                    if (plan == null)
                    {
                        Logger.GetLogger().Error($"Unable to find plan: {planId}");
                        return;
                    }

                    await planService.Run(uow, plan, curPayload.Select(x => CrateStorageSerializer.Default.ConvertFromDto(x)).ToArray());
                }
            }
            catch (InvalidTokenRuntimeException ex)
            {
                PlanDO monitoringPlan = null;

                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var plan = uow.PlanRepository.GetById<PlanDO>(planId);

                    if (plan != null && planService.IsMonitoringPlan(uow, plan))
                    {
                        monitoringPlan = plan;
                    }
                }

                if (monitoringPlan != null)
                {
                    await planService.Deactivate(planId);
                    Logger.GetLogger().Error($"Plan {planId} was deactivated due to authentication problems.");
                    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                    {
                        await planService.ReportAuthDeactivation(uow, monitoringPlan, ex);
                    }
                }
            }
            catch
            {
            }

            Logger.GetLogger().Info($"Finished executing plan {planId} as a reaction to external event");
        }

        private async Task<ContainerDO> Run(IUnitOfWork uow, PlanDO plan, Crate[] curPayload)
        {
            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }

            var activationResults = await Activate(plan.Id, false);

            if (activationResults.ValidationErrors.Count > 0)
            {
                Logger.GetLogger().Error($"Failed to run {plan.Name}:{plan.Id} plan due to activation errors.");

                return new ContainerDO
                {
                    PlanId = plan.Id,
                    State = State.Failed
                };
            }

            var container = _containerService.Create(uow, plan, curPayload);
            await _containerService.Run(uow, container);

            // Publishing message to indicate monitoring continues
            if (IsMonitoringPlan(uow, plan))
            {
                _pusherNotifier.NotifyUser(new NotificationMessageDTO
                {
                    NotificationType = NotificationType.GenericSuccess,
                    Subject = "Trigger Activated",
                    Message = "Plan execution complete. Monitoring continues.",
                    Collapsed = false
                }, plan.Fr8AccountId);
            }

            return container;
        }

        public async Task<ContainerDTO> Run(Guid planId, Crate[] payload, Guid? containerId)
        {
            var activationResults = await Activate(planId, false);
            string userName = Thread.CurrentPrincipal?.Identity?.Name;
            var currentUserId = _security.GetCurrentUser();


            if (activationResults.ValidationErrors.Count > 0)
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var plan = uow.PlanRepository.GetById<PlanDO>(planId);
                    if (plan == null)
                    {
                        throw new MissingObjectException($"Plan with Id {planId} doesn't exist");
                    }
                    var failedActivities = new List<string>();

                    foreach (var key in activationResults.ValidationErrors.Keys)
                    {
                        var activity = uow.PlanRepository.GetById<PlanNodeDO>(key) as ActivityDO;

                        if (activity != null)
                        {

                            var label = string.IsNullOrWhiteSpace(activity.Label) ? activity.ActivityTemplate?.Name : activity.Label;
                            if (label == null)
                            {
                                var template = uow.ActivityTemplateRepository.GetByKey(activity.ActivityTemplateId);
                                label = template.Name;
                            }

                            if (string.IsNullOrWhiteSpace(label))
                            {
                                label = activity.Id.ToString();
                            }

                            failedActivities.Add($"'{label}'");

                            ReportGenericValidationErrors(userName, label, plan.Name, activationResults.ValidationErrors[key]);
                        }
                    }

                    var activitiesList = string.Join(", ", failedActivities);

                    _pusherNotifier.NotifyUser(new NotificationMessageDTO
                    {
                        NotificationType = NotificationType.GenericFailure,
                        Subject = "Plan Failed",
                        Message = $"Validation failed for activities: {activitiesList} from plan \"{plan.Name}\". See activity configuration pane for details.",
                        Collapsed = false
                    }, currentUserId);
                }

                return new ContainerDTO
                {
                    PlanId = planId,
                    ValidationErrors = activationResults.ValidationErrors
                };
            }

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                ContainerDO container = null;
                var plan = uow.PlanRepository.GetById<PlanDO>(planId);

                // it container id was passed validate it
                if (containerId != null)
                {
                    container = uow.ContainerRepository.GetByKey(containerId.Value);

                    // we didn't find contaier or found container belong to the other plan.
                    if (container == null || container.PlanId != planId)
                    {
                        // that's bad. Reset containerId to run plan with new container
                        containerId = null;
                    }
                }

                PlanType? currentPlanType = null;

                try
                {
                    if (plan != null)
                    {
                        currentPlanType = IsMonitoringPlan(uow, plan) ? PlanType.Monitoring : PlanType.RunOnce;

                        if (containerId == null)
                        {
                            // There is no sense to run monitoring plans explicitly
                            // Just return empty container
                            if (currentPlanType == PlanType.Monitoring)
                            {
                                _pusherNotifier.NotifyUser(new NotificationMessageDTO
                                {
                                    NotificationType = NotificationType.GenericSuccess,
                                    Subject = "Success",
                                    Message = $"Plan \"{plan.Name}\" activated. It will wait and respond to specified external events.",
                                    Collapsed = false
                                }, currentUserId);

                                return new ContainerDTO
                                {
                                    CurrentPlanType = currentPlanType,
                                    PlanId = planId
                                };
                            }

                            container = await Run(uow, plan, payload);
                        }
                        else
                        {
                            _pusherNotifier.NotifyUser(new NotificationMessageDTO
                            {
                                NotificationType = NotificationType.GenericSuccess,
                                Subject = "Success",
                                Message = $"Continue execution of the suspended Plan \"{plan.Name}\"",
                                Collapsed = false
                            }, currentUserId);

                            await _containerService.Continue(uow, container);
                        }

                        var response = _crate.GetStorage(container.CrateStorage).FirstCrateOrDefault<OperationalStateCM>()?.Content;
                        var responseMsg = GetResponseMessage(response);

                        if (container.State != State.Failed)
                        {
                            _pusherNotifier.NotifyUser(new NotificationMessageDTO
                            {
                                NotificationType = NotificationType.GenericSuccess,
                                Subject = "Success",
                                Message = $"Complete processing for Plan \"{plan.Name}\".{responseMsg}",
                                Collapsed = false
                            }, currentUserId);
                        }
                        else
                        {
                            _pusherNotifier.NotifyUser(new NotificationMessageDTO
                            {
                                NotificationType = NotificationType.GenericFailure,
                                Subject = "Plan Failed",
                                Message = $"Failed executing plan \"{plan.Name}\"",
                                Collapsed = false
                            }, currentUserId);
                        }

                        var containerDTO = Mapper.Map<ContainerDTO>(container);
                        containerDTO.CurrentPlanType = currentPlanType;

                        return containerDTO;
                    }

                    return null;
                }
                catch (InvalidTokenRuntimeException exception)
                {
                    //this response contains details about the error that happened on some terminal and need to be shown to client
                    if (exception.ContainerDTO != null)
                    {
                        exception.ContainerDTO.CurrentPlanType = currentPlanType;
                    }

                    if (currentPlanType == PlanType.Monitoring)
                    {
                        await Deactivate(planId);
                        Logger.GetLogger().Error($"Plan {planId} was deactivated due to authentication problems.");
                        ReportAuthDeactivation(uow, plan, exception);
                    }

                    // Do not notify -- it happens in Plan.cs
                    throw;
                }
                catch (ActivityExecutionException exception)
                {
                    //this response contains details about the error that happened on some terminal and need to be shown to client
                    if (exception.ContainerDTO != null)
                    {
                        exception.ContainerDTO.CurrentPlanType = currentPlanType;
                    }

                    NotifyWithErrorMessage(exception, plan, currentUserId, exception.ErrorMessage);

                    throw;
                }
                catch (Exception e)
                {
                    var errorMessage = "An internal error has occured. Please, contact the administrator.";
                    NotifyWithErrorMessage(e, plan, currentUserId, errorMessage);
                    throw;
                }
                finally
                {
                    // THIS CODE IS HERE ONLY TO SUPPORT CURRENT UI LOGIC THAT DISPLAYS PLAN LISTS.
                    // It should be updated to show  as 'running' only:
                    //   1. Plans that have at least one executing container
                    //   2. Active monitoring plans
                    if (currentPlanType == PlanType.RunOnce)
                    {
                        using (var planStatUpdateUow = ObjectFactory.GetInstance<IUnitOfWork>())
                        {
                            planStatUpdateUow.PlanRepository.GetById<PlanDO>(planId).PlanState = PlanState.Inactive;
                            planStatUpdateUow.SaveChanges();
                        }
                    }
                }
            }
        }

        private string GetResponseMessage(OperationalStateCM response)
        {
            string responseMsg = "";
            ResponseMessageDTO responseMessage;
            if (response != null && response.CurrentActivityResponse != null && response.CurrentActivityResponse.TryParseResponseMessageDTO(out responseMessage))
            {
                if (responseMessage != null && !string.IsNullOrEmpty(responseMessage.Message))
                {
                    responseMsg = "\n" + responseMessage.Message;
                }
            }

            return responseMsg;
        }

        private void NotifyWithErrorMessage(Exception exception, PlanDO planDO, string userId, string errorMessage = "")
        {
            var messageToNotify = string.Empty;
            if (!string.IsNullOrEmpty(errorMessage))
            {
                messageToNotify = errorMessage;
            }

            _pusherNotifier.NotifyUser(new NotificationMessageDTO
            {
                NotificationType = NotificationType.GenericFailure,
                Subject = "Plan Failed",
                Message = String.Format("Plan \"{0}\" failed. {1}", planDO.Name, messageToNotify),
                Collapsed = false
            }, userId);

        }


        // We don't have place in activity configuration pane to display activity-wide configuration errors that are not binded to specific controls.
        // Report them via Action Stream.
        private void ReportGenericValidationErrors(string userId, string activityLabel, string planName, ValidationErrorsDTO validationErrors)
        {
            var genericErrors = new List<string>();

            foreach (var error in validationErrors.ValidationErrors)
            {
                if (error.ControlNames == null || error.ControlNames.Count == 0)
                {
                    genericErrors.Add(error.ErrorMessage);
                }
            }

            if (genericErrors.Count > 0)
            {
                var errors = string.Join(" ", genericErrors.Select(x =>
                {
                    if (!x.TrimEnd().EndsWith("."))
                    {
                        return x + ".";
                    }

                    return x;
                }));

                _pusherNotifier.NotifyUser(new NotificationMessageDTO
                {
                    NotificationType = NotificationType.GenericFailure,
                    Subject = "Plan Failed",
                    Message = $"Validation of activity '{activityLabel}' from plan \"{planName}\" failed: {errors}",
                    Collapsed = false
                }, userId);
            }
        }

        public PlanDO Clone(Guid planId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var currentUser = _security.GetCurrentAccount(uow);

                var targetPlan = (PlanDO)GetPlanByActivityId(uow, planId);
                if (targetPlan == null)
                {
                    return null;
                }

                var cloneTag = "Cloned From " + planId;

                //we should clone this plan for current user
                //let's clone the plan entirely
                var clonedPlan = (PlanDO)PlanTreeHelper.CloneWithStructure(targetPlan);
                clonedPlan.Description = clonedPlan.Name + " - " + "Customized for User " + currentUser.UserName + " on " + DateTime.Now;
                clonedPlan.PlanState = PlanState.Inactive;
                clonedPlan.Tag = cloneTag;
                clonedPlan.IsApp = false; // we don't want apps to duplicate when launching 
                clonedPlan.AppLaunchURL = null;

                //linearlize tree structure
                var planTree = clonedPlan.GetDescendantsOrdered();


                //let's replace old id's of cloned plan with new id's
                //and update account information
                //TODO maybe we should do something about authorization tokens too?
                Dictionary<Guid, PlanNodeDO> parentMap = new Dictionary<Guid, PlanNodeDO>();
                foreach (var planNodeDO in planTree)
                {
                    var oldId = planNodeDO.Id;
                    planNodeDO.Id = Guid.NewGuid();
                    planNodeDO.Fr8Account = currentUser;
                    //if (planNodeDO is ActivityDO)
                    //{
                    //    (planNodeDO as ActivityDO).AuthorizationTokenId = null;
                    //}
                    parentMap.Add(oldId, planNodeDO);
                    planNodeDO.ChildNodes = new List<PlanNodeDO>();
                    if (planNodeDO.ParentPlanNodeId != null)
                    {
                        PlanNodeDO newParent;
                        //find parent from old parent id map
                        if (parentMap.TryGetValue(planNodeDO.ParentPlanNodeId.Value, out newParent))
                        {
                            //replace parent id with parent's new id
                            planNodeDO.ParentPlanNodeId = newParent.Id;
                            newParent.ChildNodes.Add(planNodeDO);
                        }
                        else
                        {
                            //this should never happen
                            throw new Exception("Unable to clone plan");
                        }
                    }
                    else
                    {
                        //this should be a plan because it has null ParentId
                        uow.PlanRepository.Add(planNodeDO as PlanDO);
                    }
                }

                //lets update all existing ids in crateStorages of new activities
                //there might be some fields or crates published with sourceActivityId
                foreach (var idMap in parentMap)
                {
                    foreach (var planNodeDO in planTree)
                    {
                        var activity = planNodeDO as ActivityDO;
                        if (activity == null)
                        {
                            continue;
                        }

                        var oldId = idMap.Key.ToString();
                        var newId = idMap.Value.Id.ToString();
                        activity.CrateStorage = activity.CrateStorage.Replace(oldId, newId);
                    }
                }


                //save new cloned plan
                uow.SaveChanges();

                return clonedPlan;
            }
        }

        private void ReportAuthError(IUnitOfWork uow, Fr8AccountDO user, InvalidTokenRuntimeException ex)
        {
            var activityTemplate = ex?.FailedActivityDTO.ActivityTemplate;

            var errorMessage = $"Activity {ex?.FailedActivityDTO.Label} was unable to authenticate with remote web-service.";
            errorMessage += $"Please re-authorize {ex?.FailedActivityDTO.Label} activity " +
                    $"by clicking on the Settings dots in the upper " +
                    $"right corner of the activity and then selecting Choose Authentication. ";

            // Try getting specific the instructions provided by the terminal.
            if (!String.IsNullOrEmpty(ex.Message))
            {
                errorMessage += "Additional instructions from the terminal: ";
                errorMessage += ex.Message;
            }

            _pusherNotifier.NotifyUser(new NotificationMessageDTO
            {
                NotificationType = NotificationType.GenericFailure,
                Subject = "Plan Failed",
                Message = errorMessage,
                Collapsed = false
            }, user.Id);
        }

        private async Task ReportAuthDeactivation(IUnitOfWork uow, PlanDO plan, InvalidTokenRuntimeException ex)
        {
            var activityTemplate = ex?.FailedActivityDTO.ActivityTemplate;

            string errorMessage = $"Activity {ex?.FailedActivityDTO.Label} was unable to authenticate with remote web-service.";
            errorMessage += $"Plan \"{plan.Name}\" which contains failed activity was deactivated.";

            _pusherNotifier.NotifyUser(new NotificationMessageDTO
            {
                NotificationType = NotificationType.GenericFailure,
                Subject = "Plan Failed",
                Message = errorMessage,
                Collapsed = false
            }, plan.Fr8AccountId);

            //Sending an Email

            var account = uow.UserRepository.GetQuery().FirstOrDefault(a => a.Id == plan.Fr8AccountId);
            try
            {
                var userEmail = account.UserName;
                var emailDO = new EmailDO();
                IConfigRepository configRepository = ObjectFactory.GetInstance<IConfigRepository>();
                string fromAddress = configRepository.Get("EmailAddress_GeneralInfo");
                var emailAddressDO = uow.EmailAddressRepository.GetOrCreateEmailAddress(fromAddress);
                emailDO.From = emailAddressDO;
                emailDO.FromID = emailAddressDO.Id;
                emailDO.AddEmailRecipient(EmailParticipantType.To,
                    uow.EmailAddressRepository.GetOrCreateEmailAddress(userEmail));
                emailDO.Subject = "Your plan was deactivated due to authentication expiration";
                string htmlText = $"Plan “{plan.Name}” was deactivated due to authentication problems. <br>If you would like to keep it active, please reauthenticate <a href='{Server.ServerUrl}dashboard/plans/{plan.Id}/builder?viewMode=plan'>here</a>";
                emailDO.HTMLText = htmlText;

                uow.EnvelopeRepository.ConfigureTemplatedEmail(emailDO, "PlanDeactivated_Template");
                uow.SaveChanges();

                await ObjectFactory.GetInstance<IEmailPackager>().Send(new EnvelopeDO { Email = emailDO });
            }

            catch { Logger.GetLogger().Error($"Couldn't send email to user {account.Id} to notify him about plan deactivation"); }
        }
    }
}