using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Fr8Data.Crates;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using Hub.Interfaces;
using Hub.Managers;

namespace Hub.Services
{
    public partial class Container : Interfaces.IContainer
    {
        private readonly IUtilizationMonitoringService _utilizationMonitoringService;
        private readonly IActivityExecutionRateLimitingService _activityRateLimiter;
        private readonly IActivity _activity;
        private readonly ICrateManager _crate;

        public Container(IActivity activity, 
                         ICrateManager crateManager, 
                         IUtilizationMonitoringService utilizationMonitoringService,
                         IActivityExecutionRateLimitingService activityRateLimiter)
        {
            _utilizationMonitoringService = utilizationMonitoringService;
            _activityRateLimiter = activityRateLimiter;
            _activity = activity;
            _crate = crateManager;
        }

        public List<ContainerDO> LoadContainers(IUnitOfWork uow, PlanDO plan)
        {
            return uow.ContainerRepository.GetQuery().Where(x => x.PlanId == plan.Id).ToList();
        }
        
        public async Task Run(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            if (curContainerDO == null)
            {
                throw new ArgumentNullException("ContainerDO is null");
            }

            var storage = _crate.GetStorage(curContainerDO.CrateStorage);
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
                if (curContainerDO.CurrentActivityId == null)
                {
                    throw new InvalidOperationException("Current container has empty call stack that usually means that execution is completed. We can't run it again.");
                }

                callStack = RecoverCallStack(uow, curContainerDO);
            }

            curContainerDO.State = State.Executing;
            uow.SaveChanges();

            var executionSession = new ExecutionSession(uow, callStack, curContainerDO, _activity, _crate, _utilizationMonitoringService, _activityRateLimiter);

            await executionSession.Run();
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

                if (node is SubPlanDO)
                {
                    nodeName = "Subplan: " + ((SubPlanDO)node).Name;
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
    }
}
