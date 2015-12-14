using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Data.Crates;
using Data.Interfaces.Manifests;
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

        #region LOOP_CRUD
        private void DeleteLoopPayload(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            using (var updater = _crate.UpdateStorage(() => curContainerDO.CrateStorage))
            {
                var operationsCrate = updater.CrateStorage.CrateContentsOfType<OperationalStatusCM>().Single();
                var loop = operationsCrate.Loops.Single(l => l.Id == curContainerDO.CurrentRouteNode.Id.ToString());
                operationsCrate.Loops.Remove(loop);
            }
            uow.SaveChanges();
        }

        private bool DidLoopReceiveBreakSignal(ContainerDO curContainerDO)
        {
            var storage = _crate.GetStorage(curContainerDO.CrateStorage);
            var operationalStatus = storage.CrateContentsOfType<OperationalStatusCM>().Single();
            if (operationalStatus.Loops.Single(l => l.Id == curContainerDO.CurrentRouteNode.Id.ToString()).BreakSignalReceived)
            {
                return true;
            }
            return false;
        }

        private void CreateLoopPayload(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            using (var updater = _crate.UpdateStorage(() => curContainerDO.CrateStorage))
            {
                var operationsCrate = updater.CrateStorage.CrateContentsOfType<OperationalStatusCM>().Single();
                operationsCrate.Loops.Add(new OperationalStatusCM.LoopStatus
                {
                    BreakSignalReceived = false,
                    Id = curContainerDO.CurrentRouteNode.Id.ToString(),
                    Level = operationsCrate.Loops.Count,
                    Index = 0
                });
            }
            uow.SaveChanges();
        }

        private bool IncrementLoopIndex(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            using (var updater = _crate.UpdateStorage(() => curContainerDO.CrateStorage))
            {
                var operationsCrate = updater.CrateStorage.CrateContentsOfType<OperationalStatusCM>().Single();
                operationsCrate.Loops.Single(l => l.Id == curContainerDO.CurrentRouteNode.Id.ToString()).Index += 1;
            }
            uow.SaveChanges();

            return true;
        }

        #endregion

        private void CreateOperationalStatusPayload(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            using (var updater = _crate.UpdateStorage(() => curContainerDO.CrateStorage))
            {
                var operationalStatus = new OperationalStatusCM();
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
        /*
        private bool IsThisLoopAction(RouteNodeDO routeNode)
        {
            return routeNode is ActionDO && ((ActionDO)routeNode).ActivityTemplate.Type == ActivityType.Loop;
        }
        */
        private void SetCurrentNode(IUnitOfWork uow, ContainerDO curContainerDO, RouteNodeDO curRouteNode)
        {
            curContainerDO.CurrentRouteNode = curRouteNode;
            curContainerDO.CurrentRouteNodeId = curRouteNode != null ? curRouteNode.Id : (Guid?)null;
            uow.SaveChanges();
        }
        /*
        private async Task ExecuteActionTree(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            var me = curContainerDO.CurrentRouteNode;
            var isThisLoopAction = IsThisLoopAction(me);
            if (isThisLoopAction)
            {
                CreateLoopPayload(uow, curContainerDO);
            }

            do
            {
                //process me
                await _activity.Process(me.Id, curContainerDO);
                if (isThisLoopAction && DidLoopReceiveBreakSignal(curContainerDO))
                {
                    DeleteLoopPayload(uow, curContainerDO);
                    break;
                }
                var myFirstChild = GetFirstChild(me);
                if (myFirstChild != null) {
                    SetCurrentNode(uow, curContainerDO, myFirstChild);
                    await ExecuteActionTree(uow, curContainerDO);
                    SetCurrentNode(uow, curContainerDO, me);
                }
            } while (isThisLoopAction && IncrementLoopIndex(uow, curContainerDO));

            //switch to my sibling
            var nextSibling = GetNextSibling(me);
            if (nextSibling != null)
            {
                SetCurrentNode(uow, curContainerDO, nextSibling);
                await ExecuteActionTree(uow, curContainerDO);
            }
        }
        */
        

        private async Task ProcessChildActions(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            var me = curContainerDO.CurrentRouteNode;
            var myFirstChild = GetFirstChild(me);
            if (myFirstChild != null)
            {
                SetCurrentNode(uow, curContainerDO, myFirstChild);
                await ExecuteActionTree(uow, curContainerDO);
                SetCurrentNode(uow, curContainerDO, me);
            }
        }

        private async Task ProcessNextSiblingAction(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            var me = curContainerDO.CurrentRouteNode;
            var nextSibling = GetNextSibling(me);
            if (nextSibling != null)
            {
                SetCurrentNode(uow, curContainerDO, nextSibling);
                await ExecuteActionTree(uow, curContainerDO);
            }
        }

        private bool ShouldLoopBreak(ContainerDO curContainerDO)
        {
            var storage = _crate.GetStorage(curContainerDO.CrateStorage);
            var operationalStatus = storage.CrateContentsOfType<OperationalStatusCM>().Single();
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

        public async Task ExecuteActionTree(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            var me = curContainerDO.CurrentRouteNode;
            while(true)
            {
                await _activity.Process(me.Id, curContainerDO);
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

            await ProcessNextSiblingAction(uow, curContainerDO);
        }

        public async Task Execute(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            if (curContainerDO == null)
                throw new ArgumentNullException("ContainerDO is null");

            CreateOperationalStatusPayload(uow, curContainerDO);

            if (curContainerDO.CurrentRouteNode != null)
            {
                await ExecuteActionTree(uow, curContainerDO);
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