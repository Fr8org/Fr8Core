using AutoMapper;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Infrastructure.Security;
using Data.Repositories.Plan;
using Data.Infrastructure.StructureMap;
using Data.States;
using Fr8.Infrastructure;
using Fr8.Infrastructure.Communication;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities;
using Fr8.Infrastructure.Utilities.Logging;
using Hub.Exceptions;
using Hub.Managers;
using Hub.Interfaces;
using Hub.Managers.APIManagers.Transmitters.Terminal;

namespace Hub.Services
{
    public class Activity : IActivity
    {
        private readonly ICrateManager _crateManager;
        private readonly IAuthorization _authorizationToken;
        private readonly ISecurityServices _security;
        private readonly IActivityTemplate _activityTemplate;
        private readonly IPlanNode _planNode;
        private readonly IUpstreamDataExtractionService _upstreamDataExtractionService;
        private readonly AsyncMultiLock _configureLock = new AsyncMultiLock();

        public Activity(ICrateManager crateManager, IAuthorization authorizationToken, ISecurityServices security, IActivityTemplate activityTemplate, IPlanNode planNode, IUpstreamDataExtractionService upstreamDataExtractionService)
        {
            _crateManager = crateManager;
            _authorizationToken = authorizationToken;
            _security = security;
            _activityTemplate = activityTemplate;
            _planNode = planNode;
            _upstreamDataExtractionService = upstreamDataExtractionService;
        }

        public async Task<ActivityDTO> SaveOrUpdateActivity(ActivityDO submittedActivityData)
        {
            if (submittedActivityData.Id == Guid.Empty)
            {
                return await SaveOrUpdateActivityCore(submittedActivityData);
            }
            
            using (await _configureLock.Lock(submittedActivityData.Id))
            {
                return await SaveOrUpdateActivityCore(submittedActivityData);
            }
        }

        [AuthorizeActivity(Permission = PermissionType.ReadObject, ParamType = typeof(Guid), TargetType = typeof(PlanNodeDO))]
        public ActivityDO GetById(IUnitOfWork uow, Guid id)
        {
            return uow.PlanRepository.GetById<ActivityDO>(id);
        }

        public async Task<PlanNodeDO> CreateAndConfigure(IUnitOfWork uow, string userId, Guid activityTemplateId, string label = null, string name = null, int? order = null, Guid? parentNodeId = null, bool createPlan = false, Guid? authorizationTokenId = null, PlanVisibility newPlanVisibility = PlanVisibility.Standard)
        {
            if (parentNodeId != null && createPlan)
            {
                throw new ArgumentException("Parent node id can't be set together with create plan flag");
            }

            if (parentNodeId == null && !createPlan)
            {
                throw new ArgumentException("Either Parent node id or create plan flag must be set");
            }

            // to avoid null pointer exception while creating parent node if label is null 
            if (name == null)
            {
                name = userId + "_" + activityTemplateId;
            }

            PlanNodeDO parentNode;
            PlanDO plan = null;

            if (createPlan)
            {
                plan = ObjectFactory.GetInstance<IPlan>().Create(uow, name, ownerId: userId, visibility: newPlanVisibility);

                plan.ChildNodes.Add(parentNode = new SubplanDO
                {
                    StartingSubPlan = true,
                    Name = name + " #1"
                });
            }
            else
            {
                parentNode = uow.PlanRepository.GetById<PlanNodeDO>(parentNodeId);

                if (parentNode is PlanDO)
                {
                    if (((PlanDO)parentNode).StartingSubplan == null)
                    {
                        parentNode.ChildNodes.Add(parentNode = new SubplanDO
                        {
                            StartingSubPlan = true,
                            Name = name + " #1"
                        });
                    }
                    else
                    {
                        parentNode = ((PlanDO)parentNode).StartingSubplan;
                    }

                }
            }

            var activity = new ActivityDO
            {
                Id = Guid.NewGuid(),
                ActivityTemplateId = activityTemplateId,
                CrateStorage = _crateManager.EmptyStorageAsStr(),
                AuthorizationTokenId = authorizationTokenId
            };

            parentNode.AddChild(activity, order);

            uow.SaveChanges();

            var configuredActivity = await CallActivityConfigure(uow, userId, activity);

            UpdateActivityProperties(uow, configuredActivity);

            if (createPlan)
            {
                return plan;
            }

            return activity;
        }

        [AuthorizeActivity(Permission = PermissionType.EditObject, ParamType = typeof(ActivityDO), TargetType = typeof(PlanNodeDO))]
        public async Task<ActivityDTO> Configure(IUnitOfWork uow, string userId, ActivityDO submittedActivity)
        {
            if (submittedActivity == null)
            {
                throw new ArgumentNullException(nameof(submittedActivity));
            }

            using (await _configureLock.Lock(submittedActivity.Id))
            {
                await DeactivateActivity(uow, submittedActivity.Id);

                var configuredActivity = await CallActivityConfigure(uow, userId, submittedActivity);

                SaveAndUpdateActivity(uow, configuredActivity);

                uow.SaveChanges();

                configuredActivity = uow.PlanRepository.GetById<ActivityDO>(submittedActivity.Id);

                return Mapper.Map<ActivityDTO>(configuredActivity);
            }
        }

        [AuthorizeActivity(Permission = PermissionType.EditObject, ParamType = typeof(ActivityDO), TargetType = typeof(PlanNodeDO))]
        public async Task<ActivityDTO> Activate(ActivityDO submittedActivity)
        {
            using (await _configureLock.Lock(submittedActivity.Id))
            {
                try
                {
                    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                    {
                        var existingAction = uow.PlanRepository.GetById<ActivityDO>(submittedActivity.Id);

                        if (existingAction == null)
                        {
                            throw new Exception("Action was not found");
                        }

                        if (existingAction.ActivationState == ActivationState.Activated)
                        {
                            return Mapper.Map<ActivityDTO>(submittedActivity);
                        }

                        Logger.GetLogger().Info($"Before calling terminal activation of activity (Id - {submittedActivity.Id})");
                        var activatedActivityDTO = await CallTerminalActivityAsync<ActivityDTO>(uow, "activate", null, submittedActivity, Guid.Empty);
                        Logger.GetLogger().Info($"Call to terminal activation of activity (Id - {submittedActivity.Id}) completed");

                        var activatedActivityDo = Mapper.Map<ActivityDO>(activatedActivityDTO);
                        var storage = _crateManager.GetStorage(activatedActivityDo);
                        var validationCrate = storage.CrateContentsOfType<ValidationResultsCM>().FirstOrDefault();
                        if (validationCrate == null || !validationCrate.HasErrors)
                        {
                            existingAction.ActivationState = ActivationState.Activated;
                        }
                        UpdateActivityProperties(existingAction, activatedActivityDo);
                        uow.SaveChanges();
                        EventManager.ActionActivated(activatedActivityDo);
                        return Mapper.Map<ActivityDTO>(activatedActivityDo);
                    }
                }
                catch (Exception e)
                {
                    ReportActivityInvocationError(submittedActivity, e.Message, null, submittedActivity.Id.ToString(), EventManager.TerminalActionActivationFailed);
                    throw;
                }
            }
        }

        [AuthorizeActivity(Permission = PermissionType.EditObject, ParamType = typeof(ActivityDO), TargetType = typeof(PlanNodeDO))]
        public async Task Deactivate(ActivityDO curActivityDO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                await CallActivityDeactivate(uow, curActivityDO.Id);
                uow.SaveChanges();
            }
        }

        private async Task Delete(IUnitOfWork uow, Guid id)
        {
            //we are using Kludge solution for now
            //https://maginot.atlassian.net/wiki/display/SH/Action+Deletion+and+Reordering

            var curAction = uow.PlanRepository.GetById<ActivityDO>(id);
            if (curAction == null)
            {
                throw new MissingObjectException($"Activity with Id {id} doesn't exist");
            }

            curAction.RemoveFromParent();

            await CallActivityDeactivate(uow, curAction.Id);
        }

        [AuthorizeActivity(Permission = PermissionType.DeleteObject, ParamType = typeof(Guid), TargetType = typeof(PlanNodeDO))]
        public async Task Delete(Guid id)
        {
            //we are using Kludge solution for now
            //https://maginot.atlassian.net/wiki/display/SH/Action+Deletion+and+Reordering
            using (await _configureLock.Lock(id))
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    await Delete(uow, id);
                    uow.SaveChanges();
                }
            }
        }

        [AuthorizeActivity(Permission = PermissionType.DeleteObject, ParamType = typeof(Guid), TargetType = typeof(PlanNodeDO))]
        public async Task DeleteChildNodes(Guid id)
        {
            //we are using Kludge solution for now
            //https://maginot.atlassian.net/wiki/display/SH/Action+Deletion+and+Reordering
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curAction = uow.PlanRepository.GetById<ActivityDO>(id);
                if (curAction.ChildNodes != null)
                {
                    var childNodes = curAction.ChildNodes.OfType<ActivityDO>().ToList();
                    foreach (var childNode in childNodes)
                    {
                        using (await _configureLock.Lock(childNode.Id))
                        {
                            await Delete(uow, childNode.Id);
                        }
                    }
                }
                uow.SaveChanges();
            }
        }

        [AuthorizeActivity(Permission = PermissionType.RunObject, ParamType = typeof(ActivityDO), TargetType = typeof(PlanNodeDO))]
        public async Task<PayloadDTO> Run(IUnitOfWork uow, ActivityDO curActivityDO, ActivityExecutionMode curActionExecutionMode, ContainerDO curContainerDO)
        {
            if (curActivityDO == null)
            {
                throw new ArgumentNullException(nameof(curActivityDO));
            }

            //FR-2642 Logic to skip execution of activities with "SkipAtRunTime" Tag
            var template = _activityTemplate.GetByKey(curActivityDO.ActivityTemplateId);
            if (template.Tags != null && template.Tags.Contains("SkipAtRunTime", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            EventManager.ActionStarted(curActivityDO, curContainerDO);

            // Explicitly extract authorization token to make AuthTokenDTO pass to activities.
            curActivityDO.AuthorizationToken = uow.AuthorizationTokenRepository.FindTokenById(curActivityDO.AuthorizationTokenId);

            try
            {
                IEnumerable<KeyValuePair<string, string>> parameters = null;

                if (curActionExecutionMode != ActivityExecutionMode.InitialRun)
                {
                    parameters = new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("scope", "childActivities")
                    };
                }

                if (curActionExecutionMode != ActivityExecutionMode.ReturnFromChildren)
                {
                    EventManager.ActivityRunRequested(curActivityDO, curContainerDO);
                }

                var activtiyClone = (ActivityDO) curActivityDO.Clone();

                using (var storage = _crateManager.UpdateStorage(() => activtiyClone.CrateStorage))
                {
                    var configurationControls = storage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

                    if (configurationControls != null)
                    {
                        var payloadStorage = _crateManager.GetStorage(curContainerDO.CrateStorage);
                        _upstreamDataExtractionService.ExtactAndAssignValues(configurationControls, payloadStorage);
                    }
                }

                var payloadDTO = await CallTerminalActivityAsync<PayloadDTO>(uow, "run", parameters, activtiyClone, curContainerDO.Id);
                
                EventManager.ActivityResponseReceived(curActivityDO, ActivityResponse.RequestSuspend);

                return payloadDTO;
            }
            catch (Exception e)
            {
                ReportActivityInvocationError(curActivityDO, e.Message, curContainerDO.Id.ToString(), curActivityDO.Id.ToString(), EventManager.TerminalRunFailed);
                throw;
            }
        }

        private async Task<ActivityDTO> SaveOrUpdateActivityCore(ActivityDO submittedActivity)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                await DeactivateActivity(uow, submittedActivity.Id);

                SaveAndUpdateActivity(uow, submittedActivity);

                uow.SaveChanges();

                var result = uow.PlanRepository.GetById<ActivityDO>(submittedActivity.Id);

                return Mapper.Map<ActivityDTO>(result);
            }
        }

        private void UpdateActivityProperties(IUnitOfWork uow, ActivityDO submittedActivity)
        {
            var existingAction = uow.PlanRepository.GetById<ActivityDO>(submittedActivity.Id);

            if (existingAction == null)
            {
                throw new Exception("Action was not found");
            }

            UpdateActivityProperties(existingAction, submittedActivity);
            uow.SaveChanges();
        }

        private static void UpdateActivityProperties(ActivityDO existingActivity, ActivityDO submittedActivity)
        {
            // it is unlikely that we have scenarios when activity template can be changed after activity was created
            //existingActivity.ActivityTemplateId = submittedActivity.ActivityTemplateId;

            existingActivity.Label = submittedActivity.Label;
            existingActivity.CrateStorage = submittedActivity.CrateStorage;
            existingActivity.Ordering = submittedActivity.Ordering;
        }

        private static void RestoreSystemProperties(ActivityDO existingActivity, ActivityDO submittedActivity)
        {
            submittedActivity.ActivationState = existingActivity.ActivationState;
            submittedActivity.AuthorizationTokenId = existingActivity.AuthorizationTokenId;
        }

        private void SaveAndUpdateActivity(IUnitOfWork uow, ActivityDO submittedActiviy)
        {
            var isNewActivity = false;
            PlanTreeHelper.Visit(submittedActiviy, x =>
            {
                if (x.Id == Guid.Empty)
                {
                    if (Object.ReferenceEquals(x, submittedActiviy)) {
                        isNewActivity = true;
                    }
                    x.Id = Guid.NewGuid();
                }
                var activityDO = x as ActivityDO;
                if (activityDO != null && activityDO.ActivityTemplateId == Guid.Empty)
                {
                    var activityTemplate = activityDO.ActivityTemplate;
                    activityDO.ActivityTemplate = uow
                        .ActivityTemplateRepository
                        .GetQuery()
                        .Single(y => y.Name == activityTemplate.Name 
                                  && y.Version == activityTemplate.Version
                                  && y.Terminal.Name == activityTemplate.Terminal.Name
                                  && y.Terminal.Version == activityTemplate.Terminal.Version);
                    activityDO.ActivityTemplateId = activityDO.ActivityTemplate.Id;
                }
            });

            PlanNodeDO plan;
            PlanNodeDO originalAction;
            if (submittedActiviy.ParentPlanNodeId != null)
            {
                plan = uow.PlanRepository.Reload<PlanNodeDO>(submittedActiviy.ParentPlanNodeId);
                if (plan == null)
                {
                    throw new MissingObjectException($"Parent plan with Id {submittedActiviy.ParentPlanNodeId} doesn't exist");
                }
                originalAction = plan.ChildNodes.FirstOrDefault(x => x.Id == submittedActiviy.Id);
                
                //This might mean that this plan's parent was changed
                if (originalAction == null && !isNewActivity)
                {
                    originalAction = uow.PlanRepository.GetById<PlanNodeDO>(submittedActiviy.Id);
                    if (originalAction != null) {
                        var originalActionsParent = uow.PlanRepository.Reload<PlanNodeDO>(originalAction.ParentPlanNodeId);
                        originalActionsParent.ChildNodes.Remove(originalAction);
                        originalAction.ParentPlanNodeId = plan.Id;
                    }
                }
            }
            else
            {
                originalAction = uow.PlanRepository.Reload<PlanNodeDO>(submittedActiviy.Id);
                plan = originalAction.ParentPlanNode;
            }

            if (originalAction != null)
            {
                plan.ChildNodes.Remove(originalAction);

                // Add child subplans.
                foreach (var subPlan in originalAction.ChildNodes.OfType<SubplanDO>())
                {
                    submittedActiviy.ChildNodes.Add(subPlan);
                }

                var originalActions = PlanTreeHelper.Linearize(originalAction)
                    .OfType<ActivityDO>()
                    .ToDictionary(x => x.Id, x => x);

                foreach (var submitted in PlanTreeHelper.Linearize(submittedActiviy))
                {
                    ActivityDO existingActivity;

                    if (originalActions.TryGetValue(submitted.Id, out existingActivity))
                    {
                        RestoreSystemProperties(existingActivity, (ActivityDO)submitted);
                    }
                }
            }

            if (submittedActiviy.Ordering <= 0)
            {
                plan.AddChildWithDefaultOrdering(submittedActiviy);
            }
            else
            {
                plan.ChildNodes.Add(submittedActiviy);
            }
        }

        private async Task DeactivateActivity(IUnitOfWork uow, Guid activityId)
        {
            var exising = uow.PlanRepository.GetById<ActivityDO>(activityId);

            if (exising != null)
            {
                await DeactivateActivities(exising.GetDescendants().Select(x => x.Id));
            }
        }

        private async Task DeactivateActivities(IEnumerable<Guid> activityIds)
        {
            List<Task> tasks = new List<Task>();

            var activities = activityIds.ToArray();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                foreach (var activityId in activities)
                {
                    tasks.Add(CallActivityDeactivate(uow, activityId));
                }

                try
                {
                    await Task.WhenAll(tasks);
                    uow.SaveChanges();
                }
                catch (Exception ex)
                {
                    var failedTasks = new List<Guid>();

                    for (int index = 0; index < tasks.Count; index++)
                    {
                        var task = tasks[index];

                        if (task.Exception != null)
                        {
                            failedTasks.Add(activities[index]);
                        }
                    }

                    uow.SaveChanges();

                    throw new Exception("Following activities have failed to deactivate: " + string.Join(", ", failedTasks.Select(x => x.ToString())), ex);
                }
            }
        }

        private async Task CallActivityDeactivate(IUnitOfWork uow, Guid activityId)
        {
            var existingNode = uow.PlanRepository.GetById<PlanNodeDO>(activityId);
            if (!(existingNode is ActivityDO))
            {
                return;
            }

            var exisiting = (ActivityDO)existingNode;
            if (exisiting == null || exisiting.ActivationState == ActivationState.Deactivated)
            {
                return;
            }

            var curActivityDO = (ActivityDO)exisiting.Clone();

            var dto = Mapper.Map<ActivityDO, ActivityDTO>(curActivityDO);
            bool skipDeactivation = false;
            var template = _activityTemplate.GetByKey(curActivityDO.ActivityTemplateId);

            if (curActivityDO.AuthorizationTokenId != null || !template.NeedsAuthentication)
            {
                try
                {
                    _authorizationToken.PrepareAuthToken(uow, dto);
                }
                catch (InvalidTokenRuntimeException)
                {
                    throw;
                }
            }
            else
            {
                skipDeactivation = true;
            }

            if (!skipDeactivation)
            {
                var fr8DataDTO = new Fr8DataDTO
                {
                    ActivityDTO = dto
                };

                await ObjectFactory.GetInstance<ITerminalTransmitter>().CallActivityAsync<ActivityDTO>("deactivate", null, fr8DataDTO, Guid.Empty.ToString());
            }

            var root = exisiting.GetTreeRoot() as PlanDO;

            if (root?.PlanState == PlanState.Executing || root?.PlanState == PlanState.Active)
            {
                root.PlanState = PlanState.Inactive;
            }

            exisiting.ActivationState = ActivationState.Deactivated;
        }

        private async Task<ActivityDO> CallActivityConfigure(IUnitOfWork uow, string userId, ActivityDO submittedActivity)
        {
            var tempActionDTO = Mapper.Map<ActivityDTO>(submittedActivity);

            if (!_authorizationToken.ValidateAuthenticationNeeded(uow, userId, tempActionDTO))
            {
                submittedActivity = Mapper.Map<ActivityDO>(tempActionDTO);

                try
                {
                    tempActionDTO = await CallTerminalActivityAsync<ActivityDTO>(uow, "configure", null, submittedActivity, Guid.Empty);
                    _authorizationToken.RevokeTokenIfNeeded(uow, tempActionDTO);
                }
                catch (RestfulServiceException e)
                {
                    // terminal requested token invalidation
                    if (e.StatusCode == 419)
                    {
                        _authorizationToken.InvalidateToken(uow, userId, tempActionDTO);
                    }
                    else
                    {
                        ReportActivityInvocationError(submittedActivity, e.Message, null, submittedActivity.Id.ToString(), EventManager.TerminalConfigureFailed);
                        throw;
                    }
                }
                catch (Exception e)
                {
                    ReportActivityInvocationError(submittedActivity, e.Message, null, submittedActivity.Id.ToString(), EventManager.TerminalConfigureFailed);
                    throw;
                }
            }
            return Mapper.Map<ActivityDO>(tempActionDTO);
        }

        private async Task<TResult> CallTerminalActivityAsync<TResult>(
            IUnitOfWork uow,
            string activityName,
            IEnumerable<KeyValuePair<string, string>> parameters,
            ActivityDO curActivityDO,
            Guid containerId,
            string curDocumentationSupport = null)
        {
            if (activityName == null) throw new ArgumentNullException("activityName");
            if (curActivityDO == null) throw new ArgumentNullException("curActivityDO");

            var dto = Mapper.Map<ActivityDO, ActivityDTO>(curActivityDO);

            var fr8DataDTO = new Fr8DataDTO
            {
                ContainerId = containerId,
                ActivityDTO = dto
            };

            if (curDocumentationSupport != null)
            {
                dto.Documentation = curDocumentationSupport;
            }

            _authorizationToken.PrepareAuthToken(uow, dto);

            EventManager.ActionDispatched(curActivityDO, containerId);

            if (containerId != Guid.Empty)
            {
                var containerDO = uow.ContainerRepository.GetByKey(containerId);
                EventManager.ContainerSent(containerDO, curActivityDO);

                var response = ObjectFactory.GetInstance<ITerminalTransmitter>().CallActivityAsync<TResult>(activityName, parameters, fr8DataDTO, containerId.ToString());

                EventManager.ContainerReceived(containerDO, curActivityDO);
                return await response;
            }

            return await ObjectFactory.GetInstance<ITerminalTransmitter>().CallActivityAsync<TResult>(activityName, parameters, fr8DataDTO, containerId.ToString());
        }

        private void ReportActivityInvocationError(ActivityDO activity, string error, string containerId, string objectId, Action<string, string, string, string> reportingAction)
        {
            var endpoint = _activityTemplate.GetTerminalUrl(activity.ActivityTemplateId) ?? "<no terminal url>";

            var additionalData = containerId.IsNullOrEmpty() ? " no data " : " Container Id = " + containerId;

            reportingAction(endpoint, additionalData, error, objectId);
        }


        //This method finds and returns single SolutionPageDTO that holds some documentation of Activities that is obtained from a solution by aame
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="activityDTO"></param>
        /// <param name="isSolution">This parameter controls the access level: if it is a solution case
        /// we allow calls without CurrentAccount; if it is not - we need a User to get the list of available activities</param>
        /// <returns>Task<DocumentationResponseDTO></returns>
        public async Task<T> GetActivityDocumentation<T>(ActivityDTO activityDTO, bool isSolution = false) where T : class
        {
            //activityResponce can be either of type SolutoinPageDTO or ActivityRepsonceDTO
            T activityResponce;
            var userId = Guid.NewGuid().ToString();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var allActivityTemplates = ObjectFactory.GetInstance<IEnumerable<ActivityTemplateDTO>>();
                if (isSolution)
                    //Get the list of all actions that are solutions from database
                    allActivityTemplates = _planNode.GetSolutions(uow);
                else
                {
                    var curUser = _security.GetCurrentAccount(uow);
                    userId = curUser.Id;
                    allActivityTemplates = _planNode.GetAvailableActivities(uow, curUser);
                }
                //find the activity by the provided name

                // To prevent mismatch between db and terminal solution lists, Single or Default used
                var curActivityTerminalDTO = allActivityTemplates
                    .OrderByDescending(x => int.Parse(x.Version))
                    .FirstOrDefault(a => a.Name == activityDTO.ActivityTemplate.Name);
                //prepare an Activity object to be sent to Activity in a Terminal
                //IMPORTANT: this object will not be hold in the database
                //It is used to transfer data
                //as ActivityDTO is the first mean of communication between The Hub and Terminals

                // Since there can be mismatched data between db and terminal solution list, we should make a null check here 
                if (curActivityTerminalDTO == null)
                {
                    return null;
                }
                var curActivityDTO = new ActivityDTO
                {
                    Id = Guid.NewGuid(),
                    Label = curActivityTerminalDTO.Label,
                    ActivityTemplate = new ActivityTemplateSummaryDTO
                    {
                        Name = curActivityTerminalDTO.Name,
                        Version = curActivityTerminalDTO.Version,
                        TerminalName = curActivityTerminalDTO.Terminal.Name,
                        TerminalVersion = curActivityTerminalDTO.Terminal.Version
                    },
                    AuthToken = new AuthorizationTokenDTO
                    {
                        UserId = null
                    },
                    Documentation = activityDTO.Documentation
                };

                activityResponce = await GetDocumentation<T>(curActivityDTO);
            }
            return activityResponce;
        }

        private async Task<T> GetDocumentation<T>(ActivityDTO curActivityDTO)
        {
            //Put a method name so that HandleFr8Request could find correct method in the terminal Action
            var actionName = "documentation";
            curActivityDTO.Documentation = curActivityDTO.Documentation;
            var curContainerId = Guid.Empty;
            var fr8Data = new Fr8DataDTO
            {
                ActivityDTO = curActivityDTO
            };
            //Call the terminal
            return await ObjectFactory.GetInstance<ITerminalTransmitter>().CallActivityAsync<T>(actionName, null, fr8Data, curContainerId.ToString());
        }

        public List<string> GetSolutionNameList(string terminalName)
        {
            var solutionNameList = new List<string>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var solutionId = ActivityCategories.SolutionId;

                var curActivities = uow.ActivityTemplateRepository
                    .GetQuery()
                    .Where(x => x.Terminal.Name == terminalName
                        && x.Categories.Any(y => y.ActivityCategoryId == solutionId))
                    .GroupBy(x => x.Name)
                    .AsEnumerable()
                    .Select(x => x.OrderByDescending(y => int.Parse(y.Version)).First())
                    .ToList();
                solutionNameList.AddRange(curActivities.Select(activity => activity.Name));
            }
            return solutionNameList;
        }

        public bool Exists(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.PlanRepository.GetActivityQueryUncached().Any(x => x.Id == id);
            }
        }

        public Task<ActivityDO> GetSubordinateActivity(IUnitOfWork uow, Guid id)
        {
            var activity = uow.PlanRepository.GetById<ActivityDO>(id);
            if (activity.ChildNodes != null && activity.ChildNodes.Count == 1)
            {
                var subordinateSubPlan = activity.ChildNodes.First() as SubplanDO;
                if (subordinateSubPlan != null)
                {
                    if (subordinateSubPlan.ChildNodes != null && subordinateSubPlan.ChildNodes.Count == 1)
                    {
                        var subordinateActivity = subordinateSubPlan.ChildNodes.First() as ActivityDO;
                        if (subordinateActivity != null)
                        {
                            subordinateActivity.CrateStorage = string.Empty;
                            subordinateActivity.AuthorizationTokenId = null;
                            subordinateActivity.AuthorizationToken = null;

                            uow.SaveChanges();

                            return Task.FromResult(subordinateActivity);
                        }
                    }
                }
            }

            return Task.FromResult<ActivityDO>(null);
        }
    }
}