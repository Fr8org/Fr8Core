using AutoMapper;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Newtonsoft.Json;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Infrastructure.Security;
using Data.Repositories.Plan;
using Data.Infrastructure.StructureMap;
using Data.States;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using Hub.Infrastructure;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Terminal;
using Infrastructure.Communication;
using Utilities;

namespace Hub.Services
{
    public class Activity : IActivity
    {
        private readonly ICrateManager _crate;
        private readonly IAuthorization _authorizationToken;
        private readonly ISecurityServices _security;
        private readonly IActivityTemplate _activityTemplate;
        private readonly IPlanNode _planNode;
        private readonly AsyncMultiLock _configureLock = new AsyncMultiLock();

        public Activity(ICrateManager crate, IAuthorization authorizationToken, ISecurityServices security, IActivityTemplate activityTemplate, IPlanNode planNode)
        {
            _crate = crate;
            _authorizationToken = authorizationToken;
            _security = security;
            _activityTemplate = activityTemplate;
            _planNode = planNode;
        }
        
        public IEnumerable<TViewModel> GetAllActivities<TViewModel>()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.PlanRepository.GetActivityQueryUncached().Select(Mapper.Map<TViewModel>);
            }
        }

        private ActivityDTO SaveOrUpdateActivityCore(ActivityDO submittedActivityData)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                // SaveAndUpdateActivity(uow, submittedActivityData, new List<ActivityDO>());
                SaveAndUpdateActivity(uow, submittedActivityData);

                uow.SaveChanges();

                var result = uow.PlanRepository.GetById<ActivityDO>(submittedActivityData.Id);

                return Mapper.Map<ActivityDTO>(result);
            }
        }

        public async Task<ActivityDTO> SaveOrUpdateActivity(ActivityDO submittedActivityData)
        {
            if (submittedActivityData.Id == Guid.Empty)
            {
                return SaveOrUpdateActivityCore(submittedActivityData);
            }
           
            using (await _configureLock.Lock(submittedActivityData.Id))
            {
                return SaveOrUpdateActivityCore(submittedActivityData);
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
            existingActivity.Name = submittedActivity.Name;
            existingActivity.CrateStorage = submittedActivity.CrateStorage;
            existingActivity.Ordering = submittedActivity.Ordering;
        }

        private static void RestoreSystemProperties(ActivityDO existingActivity, ActivityDO submittedActivity)
        {
            submittedActivity.AuthorizationTokenId = existingActivity.AuthorizationTokenId;
        }

        // private void SaveAndUpdateActivity(IUnitOfWork uow, ActivityDO submittedActiviy, List<ActivityDO> pendingConfiguration)
        private void SaveAndUpdateActivity(IUnitOfWork uow, ActivityDO submittedActiviy)
        {
            PlanTreeHelper.Visit(submittedActiviy, x =>
            {
                if (x.Id == Guid.Empty)
                {
                    x.Id = Guid.NewGuid();
                }
            });

            PlanNodeDO plan;
            PlanNodeDO originalAction;
            if (submittedActiviy.ParentPlanNodeId != null)
            {
                plan = uow.PlanRepository.Reload<PlanNodeDO>(submittedActiviy.ParentPlanNodeId);
                originalAction = plan.ChildNodes.FirstOrDefault(x => x.Id == submittedActiviy.Id);
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
                foreach (var subPlan in originalAction.ChildNodes.OfType<SubPlanDO>())
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

        [AuthorizeActivity(Permission = PermissionType.ReadObject, ParamType = typeof(Guid), TargetType = typeof(PlanNodeDO))]
        public ActivityDO GetById(IUnitOfWork uow, Guid id)
        {
            return uow.PlanRepository.GetById<ActivityDO>(id);
        }

        public async Task<PlanNodeDO> CreateAndConfigure(IUnitOfWork uow, string userId, Guid activityTemplateId, string label = null, string name = null, int? order = null, Guid? parentNodeId = null, bool createPlan = false, Guid? authorizationTokenId = null)
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
                name = userId + "_" + activityTemplateId.ToString();
            }

            PlanNodeDO parentNode;
            PlanDO plan = null;

            if (createPlan)
            {
                plan = ObjectFactory.GetInstance<IPlan>().Create(uow, name);

                plan.ChildNodes.Add(parentNode = new SubPlanDO
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
                    if (((PlanDO)parentNode).StartingSubPlan == null)
                    {
                        parentNode.ChildNodes.Add(parentNode = new SubPlanDO
                        {
                            StartingSubPlan = true,
                            Name = name + " #1"
                        });
                    }
                    else
                    {
                        parentNode = ((PlanDO)parentNode).StartingSubPlan;
                    }

                }
            }

            var activity = new ActivityDO
            {
                Id = Guid.NewGuid(),
                ActivityTemplateId = activityTemplateId,
                Name = name,
                CrateStorage = _crate.EmptyStorageAsStr(),
                AuthorizationTokenId = authorizationTokenId
            };

            parentNode.AddChild(activity, order);

            uow.SaveChanges();

            await ConfigureSingleActivity(uow, userId, activity);

            if (createPlan)
            {
                return plan;
            }

            return activity;
        }

        private async Task<ActivityDO> CallActivityConfigure(IUnitOfWork uow, string userId, ActivityDO curActivityDO)
        {
            var plan = curActivityDO.RootPlanNode as PlanDO;

            if (plan?.PlanState == PlanState.Deleted)
            {
                var message = "Cannot configure activity when plan is deleted";

                EventManager.TerminalConfigureFailed(
                   _activityTemplate.GetTerminalUrl(curActivityDO.ActivityTemplateId),
                    JsonConvert.SerializeObject(Mapper.Map<ActivityDTO>(curActivityDO)),
                    message,
                    curActivityDO.Id.ToString()
                    );

                throw new ApplicationException(message);
            }

            var tempActionDTO = Mapper.Map<ActivityDTO>(curActivityDO);

            if (!_authorizationToken.ValidateAuthenticationNeeded(uow, userId, tempActionDTO))
            {
                curActivityDO = Mapper.Map<ActivityDO>(tempActionDTO);

                try
                {
                    tempActionDTO = await CallTerminalActivityAsync<ActivityDTO>(uow, "configure", curActivityDO, Guid.Empty);
                    _authorizationToken.RevokeTokenIfNeeded(uow, tempActionDTO);
                }
                catch (ArgumentException e)
                {
                    EventManager.TerminalConfigureFailed("<no terminal url>", JsonConvert.SerializeObject(Mapper.Map<ActivityDTO>(curActivityDO)), e.Message, curActivityDO.Id.ToString());
                    throw;
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
                        JsonSerializerSettings settings = new JsonSerializerSettings
                        {
                            PreserveReferencesHandling = PreserveReferencesHandling.Objects
                        };
                        var endpoint = _activityTemplate.GetTerminalUrl(curActivityDO.ActivityTemplateId) ?? "<no terminal url>";
                        EventManager.TerminalConfigureFailed(endpoint, JsonConvert.SerializeObject(Mapper.Map<ActivityDTO>(curActivityDO), settings), e.Message, curActivityDO.Id.ToString());
                        throw;
                    }
                }
                catch (Exception e)
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings
                    {
                        PreserveReferencesHandling = PreserveReferencesHandling.Objects
                    };

                    var endpoint = _activityTemplate.GetTerminalUrl(curActivityDO.ActivityTemplateId) ?? "<no terminal url>";
                    EventManager.TerminalConfigureFailed(endpoint, JsonConvert.SerializeObject(Mapper.Map<ActivityDTO>(curActivityDO), settings), e.Message, curActivityDO.Id.ToString());
                    throw;
                }
            }

            return Mapper.Map<ActivityDO>(tempActionDTO);
        }

        private async Task<ActivityDO> ConfigureSingleActivity(IUnitOfWork uow, string userId, ActivityDO curActivityDO)
        {
            curActivityDO = await CallActivityConfigure(uow, userId, curActivityDO);

            UpdateActivityProperties(uow, curActivityDO);

            return curActivityDO;
        }

        [AuthorizeActivity(Permission = PermissionType.EditObject, ParamType = typeof(ActivityDO), TargetType = typeof(PlanNodeDO))]
        public async Task<ActivityDTO> Configure(IUnitOfWork uow,
            string userId, ActivityDO curActivityDO, bool saveResult = true)
        {
            if (curActivityDO == null)
            {
                throw new ArgumentNullException(nameof(curActivityDO));
            }

            using (await _configureLock.Lock(curActivityDO.Id))
            {
                curActivityDO = await CallActivityConfigure(uow, userId, curActivityDO);

                if (saveResult)
                {
                    // SaveAndUpdateActivity(uow, curActivityDO, new List<ActivityDO>());
                    SaveAndUpdateActivity(uow, curActivityDO);

                    uow.SaveChanges();
                    curActivityDO = uow.PlanRepository.GetById<ActivityDO>(curActivityDO.Id);
                    return Mapper.Map<ActivityDTO>(curActivityDO);
                }
            }

            return Mapper.Map<ActivityDTO>(curActivityDO);
        }

        public ActivityDO MapFromDTO(ActivityDTO curActivityDTO)
        {
            ActivityDO submittedActivity = Mapper.Map<ActivityDO>(curActivityDTO);
            return submittedActivity;
        }

        [AuthorizeActivity(Permission = PermissionType.DeleteObject, ParamType = typeof(Guid), TargetType = typeof(PlanNodeDO))]
        public void Delete(Guid id)
        {
            //we are using Kludge solution for now
            //https://maginot.atlassian.net/wiki/display/SH/Action+Deletion+and+Reordering

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {

                var curAction = uow.PlanRepository.GetById<ActivityDO>(id);
                if (curAction == null)
                {
                    throw new InvalidOperationException("Unknown PlanNode with id: " + id);
                }

                var downStreamActivities = _planNode.GetDownstreamActivities(uow, curAction).OfType<ActivityDO>();
                //we should clear values of configuration controls
                var directChildren = curAction.GetDescendants().OfType<ActivityDO>();

                //there is no sense of updating children of action being deleted. 
                foreach (var downStreamActivity in downStreamActivities.Except(directChildren))
                {
                    var currentActivity = downStreamActivity;

                    using (var crateStorage = _crate.UpdateStorage(() => currentActivity.CrateStorage))
                    {
                        bool hasChanges = false;
                        foreach (var configurationControls in crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>())
                        {
                            foreach (IResettable resettable in configurationControls.Controls)
                            {
                                resettable.Reset();
                                hasChanges = true;
                            }
                        }

                        if (!hasChanges)
                        {
                            crateStorage.DiscardChanges();
                        }
                    }
                }

                curAction.RemoveFromParent();

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
                var actionName = curActionExecutionMode == ActivityExecutionMode.InitialRun ? "Run" : "ExecuteChildActivities";

                EventManager.ActivityRunRequested(curActivityDO, curContainerDO);

                var payloadDTO = await CallTerminalActivityAsync<PayloadDTO>(uow, actionName, curActivityDO, curContainerDO.Id);

                EventManager.ActivityResponseReceived(curActivityDO, ActivityResponse.RequestSuspend);

                return payloadDTO;

            }
            catch (ArgumentException e)
            {
                EventManager.TerminalRunFailed("<no terminal url>", JsonConvert.SerializeObject(Mapper.Map<ActivityDTO>(curActivityDO)), e.Message, curActivityDO.Id.ToString());
                throw;
            }
            catch (Exception e)
            {
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
                };

                var endpoint = _activityTemplate.GetTerminalUrl(curActivityDO.ActivityTemplateId) ?? "<no terminal url>";
                EventManager.TerminalRunFailed(endpoint, JsonConvert.SerializeObject(Mapper.Map<ActivityDTO>(curActivityDO), settings), e.Message, curActivityDO.Id.ToString());
                throw;
            }
        }

        [AuthorizeActivity(Permission = PermissionType.EditObject, ParamType = typeof(ActivityDO), TargetType = typeof(PlanNodeDO))]
        public async Task<ActivityDTO> Activate(ActivityDO curActivityDO)
        {
            try
            {
                //if this action contains nested actions, do not pass them to avoid 
                // circular reference error during JSON serialization (FR-1769)
                //curActivityDO = Mapper.Map<ActivityDO>(curActivityDO); // this doesn't clone activity

                curActivityDO = (ActivityDO)curActivityDO.Clone();

                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var result = await CallTerminalActivityAsync<ActivityDTO>(uow, "activate", curActivityDO, Guid.Empty);

                    EventManager.ActionActivated(curActivityDO);
                    return result;
                }
            }
            catch (ArgumentException)
            {
                EventManager.TerminalActionActivationFailed("<no terminal url>", JsonConvert.SerializeObject(Mapper.Map<ActivityDTO>(curActivityDO)), curActivityDO.Id.ToString());
                throw;
            }
            catch
            {
                EventManager.TerminalActionActivationFailed(_activityTemplate.GetTerminalUrl(curActivityDO.ActivityTemplateId) ?? "<no terminal url>", JsonConvert.SerializeObject(Mapper.Map<ActivityDTO>(curActivityDO)), curActivityDO.Id.ToString());
                throw;
            }
        }

        public async Task<ActivityDTO> Deactivate(ActivityDO curActivityDO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return await CallTerminalActivityAsync<ActivityDTO>(uow, "deactivate", curActivityDO, Guid.Empty);
            }
        }

        private Task<TResult> CallTerminalActivityAsync<TResult>(
            IUnitOfWork uow, string activityName,
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
                var reponse = ObjectFactory.GetInstance<ITerminalTransmitter>()
                    .CallActivityAsync<TResult>(activityName, fr8DataDTO, containerId.ToString());
                EventManager.ContainerReceived(containerDO, curActivityDO);
                return reponse;
            }
            return ObjectFactory.GetInstance<ITerminalTransmitter>().CallActivityAsync<TResult>(activityName, fr8DataDTO, containerId.ToString());
        }
        //This method finds and returns single SolutionPageDTO that holds some documentation of Activities that is obtained from a solution by aame
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="activityDTO"></param>
        /// <param name="isSolution">This parameter controls the access level: if it is a solution case
        /// we allow calls without CurrentAccount; if it is not - we need a User to get the list of available activities</param>
        /// <returns>Task<SolutionPageDTO/> or Task<ActivityResponceDTO/></returns>
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
                var curActivityTerminalDTO = allActivityTemplates.SingleOrDefault(a => a.Name == activityDTO.ActivityTemplate.Name);
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
                    Name = curActivityTerminalDTO.Name,
                    ActivityTemplate = curActivityTerminalDTO,
                    AuthToken = new AuthorizationTokenDTO
                    {
                        UserId = null
                    },
                    Documentation = activityDTO.Documentation
                };
                activityResponce = await GetDocumentation<T>(curActivityDTO);
                //Add log to the database
                if (!isSolution)
                {
                    var curActivityDo = Mapper.Map<ActivityDO>(activityDTO);
                    EventManager.ActivityResponseReceived(curActivityDo, ActivityResponse.ShowDocumentation);
                }

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
            return await ObjectFactory.GetInstance<ITerminalTransmitter>().CallActivityAsync<T>(actionName, fr8Data, curContainerId.ToString());
        }
        public List<string> GetSolutionNameList(string terminalName)
        {
            var solutionNameList = new List<string>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curActivities = uow.ActivityTemplateRepository.GetAll()
                    .Where(a => a.Terminal.Name == terminalName
                        && a.Category == ActivityCategory.Solution)
                        .ToList();
                solutionNameList.AddRange(curActivities.Select(activity => activity.Name));
            }
            return solutionNameList;
        }
    }
}