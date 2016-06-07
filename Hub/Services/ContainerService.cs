using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States;
using fr8.Infrastructure.Data.Crates;
using fr8.Infrastructure.Data.Managers;
using fr8.Infrastructure.Data.Manifests;
using fr8.Infrastructure.Interfaces;
using Hub.Exceptions;
using Hub.Interfaces;

namespace Hub.Services
{
    public partial class ContainerService : IContainerService
    {
        private readonly IUtilizationMonitoringService _utilizationMonitoringService;
        private readonly IActivityExecutionRateLimitingService _activityRateLimiter;
        private readonly IPusherNotifier _pusher;
        private readonly IActivity _activity;
        private readonly ICrateManager _crate;

        public ContainerService(IActivity activity, 
                         ICrateManager crateManager, 
                         IUtilizationMonitoringService utilizationMonitoringService,
                         IActivityExecutionRateLimitingService activityRateLimiter,
                         IPusherNotifier pusher)
        {
            _utilizationMonitoringService = utilizationMonitoringService;
            _activityRateLimiter = activityRateLimiter;
            _pusher = pusher;
            _activity = activity;
            _crate = crateManager;
        }

        public List<ContainerDO> LoadContainers(IUnitOfWork uow, PlanDO plan)
        {
            return uow.ContainerRepository.GetQuery().Where(x => x.PlanId == plan.Id).ToList();
        }

        /// <summary>
        /// New Process object
        /// </summary>
        /// <param name="planId"></param>
        /// <param name="envelopeId"></param>
        /// <returns></returns>
        public ContainerDO Create(IUnitOfWork uow, PlanDO curPlan, params Crate[] curPayload)
        {
            var containerDO = new ContainerDO { Id = Guid.NewGuid() };

            containerDO.PlanId = curPlan.Id;
            containerDO.Name = curPlan.Name;
            containerDO.State = State.Unstarted;

            using (var crateStorage = _crate.UpdateStorage(() => containerDO.CrateStorage))
            {
                if (curPayload?.Length > 0)
                {
                    foreach (var crate in curPayload)
                    {
                        if (crate != null && !crate.IsOfType<OperationalStateCM>())
                        {
                            crateStorage.Add(crate);
                        }
                    }
                }

                var operationalState = new OperationalStateCM();

                operationalState.CallStack.PushFrame(new OperationalStateCM.StackFrame
                {
                    NodeName = "Starting subplan",
                    NodeId = curPlan.StartingSubPlanId,
                });

                crateStorage.Add(Crate.FromContent("Operational state", operationalState));
            }

            uow.ContainerRepository.Add(containerDO);

            uow.SaveChanges();

            EventManager.ContainerCreated(containerDO);

            return containerDO;
        }

        public async Task Run(IUnitOfWork uow, ContainerDO container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("ContainerDO is null");
            }

            if (container.State == State.Failed
               || container.State == State.Completed)
            {
                throw new ApplicationException("Attempted to Launch a Container that was Failed or Completed");
            }

            var plan = uow.PlanRepository.GetById<PlanDO>(container.PlanId);
            if (plan.PlanState == PlanState.Deleted)
            {
                throw new InvalidOperationException("Can't executue container that belongs to deleted plan");
            }

            var storage = _crate.GetStorage(container.CrateStorage);
            var operationalState = storage.CrateContentsOfType<OperationalStateCM>().Single();

            if (operationalState == null)
            {
                throw new InvalidOperationException("Can't run container without properly intialized OperationalStateCM crate.");
            }

            // this is something that can't be changed by running activities. So we store current call stack for the entire run session.
            var callStack = operationalState.CallStack;

            if (callStack == null || callStack.Count == 0)
            {
                // try to convert old CurrentPlanNodeId driven logic into call stack
                if (container.CurrentActivityId == null)
                {
                    throw new InvalidOperationException("Current container has empty call stack that usually means that execution is completed. We can't run it again.");
                }

                callStack = RecoverCallStack(uow, container);
            }

            container.State = State.Executing;
            uow.SaveChanges();

            var executionSession = new ExecutionSession(uow, callStack, container, _activity, _crate, _utilizationMonitoringService, _activityRateLimiter);

            EventManager.ContainerLaunched(container);

            try
            {
                await executionSession.Run();
            }
            catch (InvalidTokenRuntimeException ex)
            {
                var user = uow.UserRepository.GetByKey(plan.Fr8AccountId);
                ReportAuthError(user, ex);
                EventManager.ContainerFailed(plan, ex, container.Id.ToString());
                throw;
            }
            catch (Exception ex)
            {
                EventManager.ContainerFailed(plan, ex, container.Id.ToString());
                throw;
            }

            EventManager.ContainerExecutionCompleted(container);
        }

        public async Task Continue(IUnitOfWork uow, ContainerDO container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (container.State != State.Suspended)
            {
                throw new ApplicationException($"Attempted to Continue a Container {container.Id} that wasn't in pending state. Container state is {container.State}.");
            }

            //continue from where we left
            await Run(uow, container);
        }

        // Previously we were using CurrentActivityId to track container execution. 
        // To better accomodate new requirements we changed CurrentActivityId to the Call Stack.
        // But we still can have pending plans in our DB that were created using CurrentActivityId as the main driven logic.
        // To be able to continue executuion of those plans we have the following method.
        // This methods takes CurrentActivityId of the plan and build a call stack using this Id and Plan structure.
        // As the result we will have correct call stack for the plan that was suspended at the node CurrentActivityId.
        // After call stack restoration CurrentActivityId is no longer needed.
        private OperationalStateCM.ActivityCallStack RecoverCallStack(IUnitOfWork uow, ContainerDO curContainerDo)
        {
            var node = uow.PlanRepository.GetById<PlanNodeDO>(curContainerDo.CurrentActivityId);

            if (node == null)
            {
                throw new InvalidOperationException($"Failed to restore call stack from CurrentPlanNodeId. Node {curContainerDo.CurrentActivityId} was not found.");
            }

            var callStack = new OperationalStateCM.ActivityCallStack();
            var pathToRoot = new List<OperationalStateCM.StackFrame>();

            while (node != null && !(node is PlanDO))
            {
                string nodeName = "undefined";

                if (node is ActivityDO)
                {
                    nodeName = "Activity: " + ((ActivityDO)node).Name;
                }

                if (node is SubplanDO)
                {
                    nodeName = "Subplan: " + ((SubplanDO)node).Name;
                }

                pathToRoot.Add(new OperationalStateCM.StackFrame
                {
                   NodeName = nodeName,
                   CurrentActivityExecutionPhase = OperationalStateCM.ActivityExecutionPhase.ProcessingChildren,
                   NodeId = node.Id
                });

                node = node.ParentPlanNode;
            }

            for (int i = pathToRoot.Count - 1; i >= 1; i --)
            {
                pathToRoot[i].CurrentChildId = pathToRoot[i - 1].NodeId;
                callStack.PushFrame(pathToRoot[i]);
            }

            callStack.PushFrame(pathToRoot[0]);
            callStack.TopFrame.CurrentActivityExecutionPhase = OperationalStateCM.ActivityExecutionPhase.WasNotExecuted;

            using (var storage = _crate.UpdateStorage(() => curContainerDo.CrateStorage))
            {
                var operationalState = storage.CrateContentsOfType<OperationalStateCM>().Single();
                operationalState.CallStack = callStack;
            }

            curContainerDo.CurrentActivityId = null;
            curContainerDo.NextActivityId = null;
            curContainerDo.CurrentPlanNode = null;
            curContainerDo.NextRouteNode = null;

            return callStack;
        }

        // Return the Containers of current Account
        public IList<ContainerDO> GetByFr8Account(IUnitOfWork unitOfWork, Fr8AccountDO account, bool isAdmin = false, Guid? id = null)
        {
            if (account.Id == null)
                throw new ApplicationException("UserId must not be null");

            var containerRepository = unitOfWork.ContainerRepository.GetQuery();


            return (id == null ? containerRepository.Where(container => container.Plan.Fr8Account.Id == account.Id) : containerRepository.Where(container => container.Id == id && container.Plan.Fr8Account.Id == account.Id)).ToList();
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
