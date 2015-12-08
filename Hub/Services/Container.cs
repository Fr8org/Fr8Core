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

        private async Task ProcessAction(ActionDO action, IUnitOfWork uow, ContainerDO curContainerDO)
        {
            switch (action.ActivityTemplate.Category)
            {
                case ActivityCategory.Loop:
                    await StartLoop(uow, curContainerDO);
                break;

                default:
                    await _activity.Process(curContainerDO.CurrentRouteNode.Id, curContainerDO);
                break;

            }
        }

        private bool ShouldBreakLoop(string loopActionId, ContainerDO curContainerDO)
        {
            var storage = _crate.GetStorage(curContainerDO.CrateStorage);
            var operationalStatus = storage.CrateContentsOfType<OperationalStatusCM>(o => o.Label == loopActionId).Single();
            if (operationalStatus.Break)
            {
                //end of this loop, let's remove this operational status crate
                using (var updater = _crate.UpdateStorage(() => curContainerDO.CrateStorage))
                {
                    updater.CrateStorage.RemoveUsingPredicate(c => c.IsOfType<OperationalStatusCM>() && c.Label == loopActionId);
                }

                return true;
            }
            return false;
        }

        private async Task StartLoop(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            var loopAction = curContainerDO.CurrentRouteNode;
            while(true)
            {
                //Process loop action
                await _activity.Process(curContainerDO.CurrentRouteNode.Id, curContainerDO);
                if (ShouldBreakLoop(curContainerDO.CurrentRouteNode.Id.ToString(), curContainerDO))
                {
                    break;
                }
                //skip loop action
                var currentAction = MoveToTheNextActivity(uow, curContainerDO) as ActionDO;

                do //process all children of loop
                {
                    await ProcessAction(currentAction, uow, curContainerDO);
                    // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
                } while ((currentAction = MoveToTheNextActivity(uow, curContainerDO) as ActionDO) != null &&
                         currentAction.ParentRouteNodeId != loopAction.ParentRouteNodeId);

                //restart loop
                curContainerDO.CurrentRouteNode = loopAction;
            } 


        }

        public async Task Execute(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            if (curContainerDO == null)
                throw new ArgumentNullException("ContainerDO is null");

            if (curContainerDO.CurrentRouteNode != null)
            {
                var currentAction = curContainerDO.CurrentRouteNode as ActionDO;
                //break if CurrentActivity Is NULL, it means all activities 
                //are processed that there is no Next Activity to set as Current Activity
                do
                {
                    await ProcessAction(currentAction, uow, curContainerDO);
                    // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
                } while ((currentAction = MoveToTheNextActivity(uow, curContainerDO) as ActionDO) != null);
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