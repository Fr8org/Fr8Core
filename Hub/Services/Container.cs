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


        /// <summary>
        /// Decides what to do with current action. if it is a loop it starts a loop operation
        /// </summary>
        /// <param name="uow"></param>
        /// <param name="curContainerDO"></param>
        /// <returns></returns>
        private async Task RouteCurrentAction(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            var currentRouteNode = curContainerDO.CurrentRouteNode;
            if (!(currentRouteNode is ActionDO))
            {
                await ProcessAction(curContainerDO);
            }
            else
            {
                switch (((ActionDO)currentRouteNode).ActivityTemplate.Type)
                {
                    case ActivityType.Loop:
                        await ProcessLoop(uow, curContainerDO);
                        break;

                    default:
                        await ProcessAction(curContainerDO);
                        break;
                }
            }
        }

        private async Task ProcessAction(ContainerDO curContainerDO)
        {
            await _activity.Process(curContainerDO.CurrentRouteNode.Id, curContainerDO);
        }

        private async Task ProcessLoop(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            var loopAction = curContainerDO.CurrentRouteNode;
            CreateLoopPayload(uow, curContainerDO);
            var lastActionOfLoop = loopAction;

            while(true)
            {
                //Process loop action
                await ProcessAction(curContainerDO);
                if (ShouldBreakLoop(curContainerDO))
                {
                    //end of this loop, let's remove this operational status crate
                    DeleteLoopPayload(uow, curContainerDO);
                    break;
                }
                //skip loop action
                var currentAction = MoveToTheNextActivity(uow, curContainerDO);

                while (currentAction != null && currentAction.ParentRouteNodeId != loopAction.ParentRouteNodeId) //process all children of loop
                {
                    await RouteCurrentAction(uow, curContainerDO);
                    var currentBackup = currentAction;
                    currentAction = MoveToTheNextActivity(uow, curContainerDO);
                    if (currentAction == null || currentAction.ParentRouteNodeId == loopAction.ParentRouteNodeId)
                    {
                        lastActionOfLoop = currentBackup;
                    }
                }

                //restart loop
                curContainerDO.CurrentRouteNode = loopAction;
                IncreaseLoopIndex(uow, curContainerDO);
            }
            //lets leave this function with currentRouteNode = lastAction of Loop
            //so next call to MoveToTheNextActivity will get sibling? of loop action
            curContainerDO.CurrentRouteNode = lastActionOfLoop;
        }

        #region LOOP_CRUD
        private void DeleteLoopPayload(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            using (var updater = _crate.UpdateStorage(() => curContainerDO.CrateStorage))
            {
                var operationsCrate = updater.CrateStorage.CrateContentsOfType<OperationalStatusCM>().Single();
                operationsCrate.RemoveLoop(curContainerDO.CurrentRouteNode.Id.ToString());
            }
            uow.SaveChanges();
        }

        private bool ShouldBreakLoop(ContainerDO curContainerDO)
        {
            var storage = _crate.GetStorage(curContainerDO.CrateStorage);
            var operationalStatus = storage.CrateContentsOfType<OperationalStatusCM>().Single();
            if (operationalStatus.GetLoopById(curContainerDO.CurrentRouteNode.Id.ToString()).Break)
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
                operationsCrate.CreateLoop(curContainerDO.CurrentRouteNode.Id.ToString());
            }
            uow.SaveChanges();
        }

        private void IncreaseLoopIndex(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            using (var updater = _crate.UpdateStorage(() => curContainerDO.CrateStorage))
            {
                var operationsCrate = updater.CrateStorage.CrateContentsOfType<OperationalStatusCM>().Single();
                operationsCrate.GetLoopById(curContainerDO.CurrentRouteNode.Id.ToString()).IncreaseLoopIndex();
            }
            uow.SaveChanges();
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

        public async Task Execute(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            if (curContainerDO == null)
                throw new ArgumentNullException("ContainerDO is null");

            CreateOperationalStatusPayload(uow, curContainerDO);

            if (curContainerDO.CurrentRouteNode != null)
            {
                //break if CurrentActivity Is NULL, it means all activities 
                //are processed that there is no Next Activity to set as Current Activity
                do
                {
                    await RouteCurrentAction(uow, curContainerDO);
                } while (MoveToTheNextActivity(uow, curContainerDO) != null);
            }
            else
            {
                throw new ArgumentNullException("CurrentActivity is null. Cannot execute CurrentActivity");
            }
        }

        private RouteNodeDO MoveToTheNextActivity(IUnitOfWork uow, ContainerDO curContainerDo)
        {
            var next =  ObjectFactory.GetInstance<IRouteNode>().GetNextActivity(curContainerDo.CurrentRouteNode, null);

            // very simple check for cycles
            if (next != null && next == curContainerDo.CurrentRouteNode)
                {
                throw new Exception(string.Format("Cycle detected. Current activty is {0}", curContainerDo.CurrentRouteNode.Id));
            }
            
            curContainerDo.CurrentRouteNode = next;

            uow.SaveChanges();

            return next;
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