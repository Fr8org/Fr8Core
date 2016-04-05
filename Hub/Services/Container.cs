using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Crates;
using Data.Interfaces.Manifests;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Hub.Interfaces;
using Hub.Managers;

namespace Hub.Services
{
    public partial class Container : Hub.Interfaces.IContainer
    {
        // Declarations
        private readonly IActivity _activity;
        private readonly ICrateManager _crate;

        public Container()
        {
            _activity = ObjectFactory.GetInstance<IActivity>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
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
                if (curContainerDO.CurrentPlanNodeId == null)
                {
                    throw new InvalidOperationException("Current container has empty call stack that usually means that execution is completed. We can't run it again.");
                }

                callStack = RecoverCallStack(uow, curContainerDO);
            }

            curContainerDO.ContainerState = ContainerState.Executing;
            uow.SaveChanges();

            var executionSession = new ExecutionSession(uow, callStack, curContainerDO, _activity, _crate);

            await executionSession.Run();
        }

        // Convert "old" execution data to new
        private OperationalStateCM.ActivityCallStack RecoverCallStack(IUnitOfWork uow, ContainerDO curContainerDo)
        {
            var node = uow.PlanRepository.GetById<PlanNodeDO>(curContainerDo.CurrentPlanNodeId);

            if (node == null)
            {
                throw new InvalidOperationException($"Failed to restore call stack from CurrentPlanNodeId. Node {curContainerDo.CurrentPlanNodeId} was not found.");
            }

            var callStack = new OperationalStateCM.ActivityCallStack();
            var pathToRoot = new List<OperationalStateCM.StackFrame>();

            while (node != null && !(node is PlanDO))
            {
                string nodeName = "undefined";

                if (node is ActivityDO)
                {
                    nodeName = "Activity: " + ((ActivityDO)node).Label;
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
                callStack.Push(pathToRoot[i]);
            }

            callStack.Push(pathToRoot[0]);

            callStack.Peek().CurrentActivityExecutionPhase = OperationalStateCM.ActivityExecutionPhase.WasNotExecuted;

            using (var storage = _crate.UpdateStorage(() => curContainerDo.CrateStorage))
            {
                var operationalState = storage.CrateContentsOfType<OperationalStateCM>().Single();
                operationalState.CallStack = callStack;
            }

            curContainerDo.CurrentPlanNodeId = null;
            curContainerDo.NextRouteNodeId = null;
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
