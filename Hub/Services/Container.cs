using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Data.Crates;
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

        private async Task StartLoop(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            var loopAction = curContainerDO.CurrentRouteNode;
            int a = 0;
            do
            {
                var currentAction = MoveToTheNextActivity(uow, curContainerDO) as ActionDO;
                do
                {
                    //TODO change this mechanism
                    if (currentAction.Label == "Create Loop")
                    {
                        await StartLoop(uow, curContainerDO);
                    }
                    else
                    {
                        await _activity.Process(curContainerDO.CurrentRouteNode.Id, curContainerDO);
                    }
                    // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
                } while ((currentAction = MoveToTheNextActivity(uow, curContainerDO) as ActionDO) != null && currentAction.ParentRouteNodeId != loopAction.ParentRouteNodeId);

                curContainerDO.CurrentRouteNode = loopAction;
                currentAction = loopAction as ActionDO;
                a++;
            } while (a < 3); //check loop ending condition here


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
                    //TODO change this mechanism
                    if (currentAction.Label == "Create Loop")
                    {
                        await StartLoop(uow, curContainerDO);
                    }
                    else { 
                        await _activity.Process(curContainerDO.CurrentRouteNode.Id, curContainerDO);
                    }
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