using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Data.Constants;
using Data.Crates;
using Data.Interfaces.Manifests;
using Hub.Exceptions;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hub.Interfaces;
using Data.Infrastructure;
using Hub.Managers;

namespace Hub.Services
{
    public class Container : Hub.Interfaces.IContainer
    {
        
        // Declarations
        
        private readonly IProcessNode _processNode;
        private readonly IRouteNode _activity;
        private readonly ICrateManager _crate;
        
        public Container()
        {
            _processNode = ObjectFactory.GetInstance<IProcessNode>();
            _activity = ObjectFactory.GetInstance<IRouteNode>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
        }

        private void AddOperationalStateCrate(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            using (var updater = _crate.UpdateStorage(() => curContainerDO.CrateStorage))
            {
                var operationalStatus = new OperationalStateCM();
                var operationsCrate = Crate.FromContent("Operational Status", operationalStatus);
                updater.CrateStorage.Add(operationsCrate);
            }

            uow.SaveChanges();
        }

        private RouteNodeDO GetFirstChild(RouteNodeDO routeNode)
        {
            var next = _activity.GetFirstChild(routeNode);
            if (next != null && next == routeNode)
            {
                throw new Exception(string.Format("Cycle detected. Current activty is {0}", routeNode.Id));
            }

            return next;
        }

        private RouteNodeDO GetNextSibling(RouteNodeDO routeNode)
        {
            var next = _activity.GetNextSibling(routeNode);
            if (next != null && next == routeNode)
            {
                throw new Exception(string.Format("Cycle detected. Current activty is {0}", routeNode.Id));
            }

            return next;
        }

        private void SetCurrentNode(IUnitOfWork uow, ContainerDO curContainerDO, RouteNodeDO curRouteNode)
        {
            curContainerDO.CurrentRouteNode = curRouteNode;
            curContainerDO.CurrentRouteNodeId = curRouteNode != null ? curRouteNode.Id : (Guid?)null;
            uow.SaveChanges();
        }        

        private async Task ProcessChildActions(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            var me = curContainerDO.CurrentRouteNode;
            var currentChild = GetFirstChild(me);
            while (currentChild != null)
            {
                SetCurrentNode(uow, curContainerDO, currentChild);
                await ProcessActionTree(uow, curContainerDO);
                currentChild = GetNextSibling(currentChild);
            }
            SetCurrentNode(uow, curContainerDO, me);
        }

        private bool ShouldLoopBreak(ContainerDO curContainerDO)
        {
            var storage = _crate.GetStorage(curContainerDO.CrateStorage);
            var operationalStatus = storage.CrateContentsOfType<OperationalStateCM>().Single();
            if (operationalStatus.Loops.Single(l => l.Id == curContainerDO.CurrentRouteNode.Id.ToString()).BreakSignalReceived)
            {
                return true;
            }
            return false;
        }

        private bool ActionProcessingComplete(ContainerDO curContainerDO)
        {
            var curRouteNode = curContainerDO.CurrentRouteNode as ActionDO;
            if (curRouteNode != null && curRouteNode.ActivityTemplate.Type == ActivityType.Loop && !ShouldLoopBreak(curContainerDO))
            {
                return false;
            }

            return true;
        }

        private bool ShouldProcessChildren(ContainerDO curContainerDO)
        {
            var curRouteNode = curContainerDO.CurrentRouteNode as ActionDO;

            if (curRouteNode != null && curRouteNode.ActivityTemplate.Type == ActivityType.Loop && ShouldLoopBreak(curContainerDO))
            {
                return false;
            }

            return true;
        }

        private ActionState? GetCurrentActionState(ContainerDO curContainerDO)
        {
            var storage = _crate.GetStorage(curContainerDO.CrateStorage);
            var operationalState = storage.CrateContentsOfType<OperationalStateCM>().Single();
            var currentActionState =
                operationalState.States.FirstOrDefault(s => s.Id == curContainerDO.CurrentRouteNode.Id.ToString());
            if (currentActionState == null)
            {
                return null;
            }

            return currentActionState.State;
        }

        private Guid GetPausedRouteNodeId(ContainerDO curContainerDO)
        {
            var storage = _crate.GetStorage(curContainerDO.CrateStorage);
            var operationalState = storage.CrateContentsOfType<OperationalStateCM>().Single();
            return operationalState.PausedRouteNodeId;
        }

        private bool ShouldProcessCurrentAction(ContainerDO curContainerDO)
        {
            //TODO should we mind other states of container?????
            if (curContainerDO.ContainerState != ContainerState.Pending)
            {
                return true;
            }//container won't process already processed actions. it continues until paused action is found. this is used to create same stack before pause
            else if (GetPausedRouteNodeId(curContainerDO) == curContainerDO.CurrentRouteNode.Id)
            {
                
                curContainerDO.ContainerState = ContainerState.Executing;
                return true;
            }
            return false;
        }

        private void MarkCurrentActionAsPausedAction(IUnitOfWork uow, ContainerDO curContainerDo)
        {
            using (var updater = _crate.UpdateStorage(() => curContainerDo.CrateStorage))
            {
                var operationalState = updater.CrateStorage.CrateContentsOfType<OperationalStateCM>().Single();
                operationalState.PausedRouteNodeId = curContainerDo.CurrentRouteNode.Id;
            }

            uow.SaveChanges();
        }

        /// <summary>
        /// For actions who don't bother with returning a state. 
        /// We will assume those actions are completed without a problem
        /// </summary>
        /// <param name="uow"></param>
        /// <param name="curContainerDo"></param>
        private void MarkCurrentActionAsCompleted(IUnitOfWork uow, ContainerDO curContainerDo)
        {
            using (var updater = _crate.UpdateStorage(() => curContainerDo.CrateStorage))
            {
                var operationalState = updater.CrateStorage.CrateContentsOfType<OperationalStateCM>().Single();
                operationalState.States.Add(new OperationalStateCM.ActionStateMatch
                {
                    Id = curContainerDo.CurrentRouteNode.Id.ToString(),
                    State = ActionState.Completed
                });
            }

            uow.SaveChanges();
        }

        private void ProcessCurrentActionState(IUnitOfWork uow, ContainerDO curContainerDo)
        {
            switch (GetCurrentActionState(curContainerDo))
            {
                case ActionState.Completed:
                    //do nothing
                    break;
                case ActionState.Pending:
                    MarkCurrentActionAsPausedAction(uow, curContainerDo);
                    throw new ExecutionPausedException();
                case null:
                    MarkCurrentActionAsCompleted(uow, curContainerDo);
                    break;
                default:
                    throw new Exception("Unknown action state on action with id " + curContainerDo.CurrentRouteNode.Id);
            }
        }

        public async Task ProcessActionTree(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            var me = curContainerDO.CurrentRouteNode;
            while(true)
            {
                //this function is called to find last action we were paused - if we were paused at all
                if (ShouldProcessCurrentAction(curContainerDO))
                {
                    await _activity.Process(me.Id, curContainerDO);
                    ProcessCurrentActionState(uow, curContainerDO);
                }
                
                if (!ShouldProcessChildren(curContainerDO))
                {
                    break;
                }
                await ProcessChildActions(uow, curContainerDO);
                if (ActionProcessingComplete(curContainerDO))
                {
                    break;
                }
            }
        }

        private bool HasOperationalStateCrate(ContainerDO curContainerDO)
        {
            var storage = _crate.GetStorage(curContainerDO.CrateStorage);
            var operationalState = storage.CrateContentsOfType<OperationalStateCM>().FirstOrDefault();
            return operationalState != null;
        }

        public async Task Run(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            if (curContainerDO == null)
                throw new ArgumentNullException("ContainerDO is null");

            //if payload already has operational state create we shouldn't create another
            if (!HasOperationalStateCrate(curContainerDO))
            {
                AddOperationalStateCrate(uow, curContainerDO);
                uow.SaveChanges();
            }

            if (curContainerDO.ContainerState == ContainerState.Unstarted)
            {
                curContainerDO.ContainerState = ContainerState.Executing;
                uow.SaveChanges();
            }


            if (curContainerDO.CurrentRouteNode != null)
            {
                await ProcessActionTree(uow, curContainerDO);
                //to mark container as finished
                SetCurrentNode(uow, curContainerDO, null);
            }
            else
            {
                throw new ArgumentNullException("CurrentActivity is null. Cannot execute CurrentActivity");
            }
        }

        // Return the Containers of current Account
        public IList<ContainerDO> GetByFr8Account(IUnitOfWork unitOfWork, Fr8AccountDO account, bool isAdmin = false, Guid? id = null)
        {
            if (account.Id == null)
                throw new ApplicationException("UserId must not be null");

            var containerRepository = unitOfWork.ContainerRepository.GetQuery();

            if (isAdmin)
            {
                return (id == null
               ? containerRepository
               : containerRepository.Where(container => container.Id == id)).ToList();
            }

            return (id == null
               ? containerRepository.Where(container => container.Route.Fr8Account.Id == account.Id)
               : containerRepository.Where(container => container.Id == id && container.Route.Fr8Account.Id == account.Id)).ToList();

        }
    }
}