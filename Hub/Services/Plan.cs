using System;
using System.Collections.Generic;
using System.Linq;
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
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using Fr8Data.States;
using Hub.Exceptions;
using Utilities.Logging;
using Utilities.Interfaces;

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
        private readonly IPusherNotifier _pusher;

        public Plan(InternalInterface.IContainerService containerService, Fr8Account dockyardAccount, IActivity activity,
            ICrateManager crate, ISecurityServices security, IJobDispatcher dispatcher, IPusherNotifier pusher)
        {
            _containerService = containerService;
            _dockyardAccount = dockyardAccount;
            _activity = activity;
            _crate = crate;
            _security = security;
            _dispatcher = dispatcher;
            _pusher = pusher;
        }

        public PlanResultDTO GetForUser(IUnitOfWork unitOfWork, Fr8AccountDO account, PlanQueryDTO planQueryDTO, bool isAdmin = false)
        {
            //lets make sure our inputs are correct
            planQueryDTO = planQueryDTO ?? new PlanQueryDTO();
            planQueryDTO.Page = planQueryDTO.Page ?? 1;
            planQueryDTO.Page = planQueryDTO.Page < 1 ? 1 : planQueryDTO.Page;
            planQueryDTO.PlanPerPage = planQueryDTO.PlanPerPage ?? DefaultPlanPageSize;
            planQueryDTO.PlanPerPage = planQueryDTO.PlanPerPage < MinPlanPageSize ? MinPlanPageSize : planQueryDTO.PlanPerPage;
            planQueryDTO.IsDescending = planQueryDTO.IsDescending ?? true;

            var planQuery = unitOfWork.PlanRepository.GetPlanQueryUncached()
                .Where(x => x.Visibility == PlanVisibility.Standard);

            planQuery = planQueryDTO.Id == null
                ? planQuery.Where(pt => pt.Fr8Account.Id == account.Id)
                : planQuery.Where(pt => pt.Id == planQueryDTO.Id && pt.Fr8Account.Id == account.Id);

            planQuery = !string.IsNullOrEmpty(planQueryDTO.Category)
                ? planQuery.Where(c => c.Category == planQueryDTO.Category)
                : planQuery.Where(c => string.IsNullOrEmpty(c.Category));

            if (!string.IsNullOrEmpty(planQueryDTO.Filter))
            {
                planQuery = planQuery.Where(c => c.Name.Contains(planQueryDTO.Filter) || c.Description.Contains(planQueryDTO.Filter));
            }

            planQuery = planQueryDTO.Status == null
                ? planQuery.Where(pt => pt.PlanState != PlanState.Deleted)
                : planQuery.Where(pt => pt.PlanState == planQueryDTO.Status);

            //lets allow ordering with just name for now
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
                Plans = planQuery.ToList().Select(Mapper.Map<PlanEmptyDTO>).ToList(),
                CurrentPage = planQueryDTO.Page.Value,
                TotalPlanCount = totalPlanCountForCurrentCriterias
            };

        }

        public bool IsMonitoringPlan(IUnitOfWork uow, PlanDO plan)
        {
            var initialActivity = plan.StartingSubPlan.GetDescendantsOrdered()
                .OfType<ActivityDO>()
                .FirstOrDefault(x => uow.ActivityTemplateRepository.GetByKey(x.ActivityTemplateId).Category != ActivityCategory.Solution);

            if (initialActivity == null)
            {
                return false;
            }

            var activityTemplate = uow.ActivityTemplateRepository.GetByKey(initialActivity.ActivityTemplateId);

            if (activityTemplate.Category == ActivityCategory.Monitors)
            {
                return true;
            }

            var storage = _crate.GetStorage(initialActivity.CrateStorage);

            // first activity has event subsribtions. This means that this plan can be triggered by external event
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

                submittedPlan.ChildNodes.Add(new SubPlanDO(true)
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
            }
        }

        public PlanDO GetFullPlan(IUnitOfWork uow, Guid id)
        {
            return uow.PlanRepository.GetById<PlanDO>(id);
        }

        public PlanDO Create(IUnitOfWork uow, string name, string category = "")
        {
            var plan = new PlanDO
            {
                Id = Guid.NewGuid(),
                Name = name,
                Fr8Account = _security.GetCurrentAccount(uow),
                PlanState = PlanState.Inactive,
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
                            ReportAuthError(plan.Fr8Account, (InvalidTokenRuntimeException)e);
                        }
                    }

                    EventManager.PlanActivationFailed(plan, "Unable to activate plan");
                    throw;
                }
                catch (InvalidTokenRuntimeException ex)
                {
                    ReportAuthError(plan.Fr8Account, (InvalidTokenRuntimeException)ex);
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

                if (result.ValidationErrors.Count == 0 && plan.PlanState != PlanState.Running)
                {
                    plan.PlanState = PlanState.Running;
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

        public async Task Deactivate(Guid planId)
        {
            var deactivateTasks = new List<Task>();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = uow.PlanRepository.GetById<PlanDO>(planId);

                plan.PlanState = PlanState.Inactive;
                uow.SaveChanges();

                foreach (var activity in plan.GetDescendants().OfType<ActivityDO>())
                {
                    deactivateTasks.Add(_activity.Deactivate(activity));
                }
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
                //get the 1st activity
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
            //We convert incoming data to DTO objects because HangFire will serialize method parameters into JSON and serializing of Crate objects is forbidden
            var curEventReportDTO = curEventReport.Select(x => CrateStorageSerializer.Default.ConvertToDto(x)).ToArray();
            //We don't await this call as it will be awaited inside HangFire after job is launched
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
             _dispatcher.Enqueue(() => LaunchPlanCallback(curPlanId, curEventReportDTO));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
        
        public static async Task LaunchPlanCallback(Guid planId, params CrateDTO[] curPayload)
        {
            Logger.LogInfo($"Starting executing plan {planId} as a reaction to external event");

            if (planId == default(Guid))
            {
                Logger.LogError("Can't lanunch plan with empty id");
            }

            // we "eat" this exception to make Hangfire thinks that everthying is good and job is completed
            // this exception should be already logged somewhere
            try
            {
                var planService = ObjectFactory.GetInstance<IPlan>();

                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var plan = uow.PlanRepository.GetById<PlanDO>(planId);

                    if (plan == null)
                    {
                        Logger.LogError($"Unable to find plan: {planId}");
                        return;
                    }
                    
                    await planService.Run(uow, plan, curPayload.Select(x => CrateStorageSerializer.Default.ConvertFromDto(x)).ToArray());
                }
            }
            catch
            {
            }

            Logger.LogInfo($"Finished executing plan {planId} as a reaction to external event");
        }

        public async Task<ContainerDO> Run(IUnitOfWork uow, PlanDO plan, Crate[] curPayload)
        {
            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }

            var activationResults = await Activate(plan.Id, false);

            if (activationResults.ValidationErrors.Count > 0)
            {
                Logger.LogError($"Failed to run {plan.Name}:{plan.Id} plan due to activation errors.");

                return new ContainerDO
                {
                    PlanId = plan.Id,
                    State = State.Failed
                };
            }

            var container = _containerService.Create(uow, plan, curPayload);
            await _containerService.Run(uow, container);

            return container;
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


                //save new cloned plan
                uow.SaveChanges();

                return clonedPlan;
            }
        }

        private void ReportAuthError(Fr8AccountDO user, InvalidTokenRuntimeException ex)
        {
            string errorMessage = $"Activity {ex?.FailedActivityDTO.Label} was unable to authenticate with " +
                    $"{ex?.FailedActivityDTO.ActivityTemplate.WebService.Name}. ";

            errorMessage += $"Please re-authorize Fr8 to connect to {ex?.FailedActivityDTO.ActivityTemplate.WebService.Name} " +
                    $"by clicking on the Settings dots in the upper " +
                    $"right corner of the activity and then selecting Choose Authentication. ";

            // Try getting specific the instructions provided by the terminal.
            if (!String.IsNullOrEmpty(ex.Message))
            {
                errorMessage += "Additional instructions from the terminal: ";
                errorMessage += ex.Message;
            }

            _pusher.NotifyUser(errorMessage, NotificationChannel.GenericFailure, user.UserName);
        }
    }
}