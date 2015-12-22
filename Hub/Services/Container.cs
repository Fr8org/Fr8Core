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

        private bool HasChildren(RouteNodeDO routeNode)
        {
            return _activity.HasChildren(routeNode);
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

        private RouteNodeDO GetParent(RouteNodeDO routeNode)
        {
            return routeNode.ParentRouteNode;
        }


        private ActionResponse GetCurrentActionResponse(ContainerDO curContainerDO)
        {
            var storage = _crate.GetStorage(curContainerDO.CrateStorage);
            var operationalState = storage.CrateContentsOfType<OperationalStateCM>().Single();
            return operationalState.CurrentActionResponse;
        }

        /// <summary>
        /// For actions who don't bother with returning a state. 
        /// We will assume those actions are completed without a problem
        /// </summary>
        /// <param name="uow"></param>
        /// <param name="curContainerDo"></param>
        private void ResetActionResponse(IUnitOfWork uow, ContainerDO curContainerDo)
        {
            using (var updater = _crate.UpdateStorage(() => curContainerDo.CrateStorage))
            {
                var operationalState = updater.CrateStorage.CrateContentsOfType<OperationalStateCM>().Single();
                operationalState.CurrentActionResponse = ActionResponse.Null;
            }

            uow.SaveChanges();
        }

        private void ProcessCurrentActionResponse(IUnitOfWork uow, ContainerDO curContainerDo, ActionResponse response)
        {
            switch (response)
            {
                case ActionResponse.Success:
                    ResetActionResponse(uow, curContainerDo);
                    //do nothing
                    break;
                case ActionResponse.RequestSuspend:
                    throw new ExecutionPausedException();
                case ActionResponse.Null:
                    //let's assume this is success for now
                    break;
                case ActionResponse.Error:
                    //TODO retry action execution until 3 errors??
                    throw new Exception("Error on action with id " + curContainerDo.CurrentRouteNode.Id);
                case ActionResponse.RequestTerminate:
                    throw new Exception("Termination request from action with id " + curContainerDo.CurrentRouteNode.Id);
                default:
                    throw new Exception("Unknown action state on action with id " + curContainerDo.CurrentRouteNode.Id);
            }
        }


        /* 
        *          a
        *       b     c 
        *     d   E  f  g  
        * 
        * 
        * We traverse this tree in this order a-b-d-E-b-c-f-g-c-a-NULL 
        */
        /// <summary>
        /// Moves to next Route
        /// </summary>
        /// <param name="uow"></param>
        /// <param name="curContainerDO"></param>
        /// <param name="skipChildren"></param>
        private ActionState MoveToNextRoute(IUnitOfWork uow, ContainerDO curContainerDO, bool skipChildren)
        {
            var state = ActionState.InitialRun;
            
            if (skipChildren || !HasChildren(curContainerDO.CurrentRouteNode))
            {
                var nextSibling = GetNextSibling(curContainerDO.CurrentRouteNode);
                if (nextSibling == null)
                {
                    var parent = GetParent(curContainerDO.CurrentRouteNode);
                    curContainerDO.CurrentRouteNode = parent;
                    curContainerDO.CurrentRouteNodeId = parent != null ? parent.Id : (Guid?)null;
                    state = ActionState.ReturnFromChildren;
                }
                else
                {
                    curContainerDO.CurrentRouteNode = nextSibling;
                    curContainerDO.CurrentRouteNodeId = nextSibling.Id;
                }
                
            }
            else
            {
                var firstChild = GetFirstChild(curContainerDO.CurrentRouteNode);
                curContainerDO.CurrentRouteNode = firstChild;
                curContainerDO.CurrentRouteNodeId = curContainerDO.CurrentRouteNode.Id;
            }
            uow.SaveChanges();

            return state;
        }

        /// <summary>
        /// Run current action and return it's response
        /// </summary>
        /// <param name="uow"></param>
        /// <param name="curContainerDO"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        private async Task<ActionResponse> ProcessAction(IUnitOfWork uow, ContainerDO curContainerDO, ActionState state)
        {
            await _activity.Process(curContainerDO.CurrentRouteNode.Id, state, curContainerDO);
            return GetCurrentActionResponse(curContainerDO);
        }

        private bool ShouldSkipChildren(ContainerDO curContainerDO, ActionState state, ActionResponse response)
        {
            //first let's check if there is a child action related response
            if (response == ActionResponse.SkipChildren)
            {
                return true;
            }
            else if (response == ActionResponse.ReProcessChildren)
            {
                return false;
            }

            //otherwise we will assume this is a regular action
            //so we will process it's children once

            if (state == ActionState.InitialRun)
            {
                return false;
            }
            else if (state == ActionState.ReturnFromChildren)
            {
                return true;
            }

            throw new Exception("This shouldn't happen");
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

            if (curContainerDO.CurrentRouteNode == null)
            {
                throw new ArgumentNullException("CurrentActivity is null. Cannot execute CurrentActivity");
            }

            var actionState = ActionState.InitialRun;
            while (curContainerDO.CurrentRouteNode != null)
            {
                var actionResponse = await ProcessAction(uow, curContainerDO, actionState);
                ProcessCurrentActionResponse(uow, curContainerDO, actionResponse);
                var shouldSkipChildren = ShouldSkipChildren(curContainerDO, actionState, actionResponse);
                actionState = MoveToNextRoute(uow, curContainerDO, shouldSkipChildren);
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