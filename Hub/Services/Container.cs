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
        private readonly IRoute _route;
        private readonly ICrateManager _crate;
        
        

        public Container()
        {
            _processNode = ObjectFactory.GetInstance<IProcessNode>();
            _activity = ObjectFactory.GetInstance<IRouteNode>();
            _route = ObjectFactory.GetInstance<IRoute>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
        }

        

        /// <summary>
        /// New Process object
        /// </summary>
        /// <param name="processTemplateId"></param>
        /// <param name="envelopeId"></param>
        /// <returns></returns>
        public ContainerDO Create(IUnitOfWork uow, int processTemplateId, Crate curEvent)
        {
            var containerDO = new ContainerDO();
            containerDO.Id = Guid.NewGuid();

            var curRoute = uow.RouteRepository.GetByKey(processTemplateId);
            if (curRoute == null)
                throw new ArgumentNullException("processTemplateId");
            containerDO.Route = curRoute;

            containerDO.Name = curRoute.Name;
            containerDO.ContainerState = ContainerState.Unstarted;
           
            if (curEvent != null)
            {
                using (var updater = _crate.UpdateStorage(() => containerDO.CrateStorage))
                {
                    updater.CrateStorage.Add(curEvent);
                }
            }

            containerDO.CurrentRouteNode = _route.GetInitialActivity(uow, curRoute);

            uow.ContainerRepository.Add(containerDO);
            uow.SaveChanges();

            //then create process node
            var subrouteId = containerDO.Route.StartingSubroute.Id;

            var curProcessNode = _processNode.Create(uow, containerDO.Id, subrouteId, "process node");
            containerDO.ProcessNodes.Add(curProcessNode);

            uow.SaveChanges();
            EventManager.ContainerCreated(containerDO);

            return containerDO;
        }



        public async Task Launch(RouteDO curRoute, Crate curEvent)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curContainerDO = Create(uow, curRoute.Id, curEvent);

            if (curContainerDO.ContainerState == ContainerState.Failed || curContainerDO.ContainerState == ContainerState.Completed)
                {
                throw new ApplicationException("Attempted to Launch a Process that was Failed or Completed");
                }

                curContainerDO.ContainerState = ContainerState.Executing;
                uow.SaveChanges();

                try
                {
                    await Execute(uow, curContainerDO);
                    curContainerDO.ContainerState = ContainerState.Completed;
                }
                catch
                {
                    curContainerDO.ContainerState = ContainerState.Failed;
                    throw;
                }
                finally
                {
                    curContainerDO.CurrentRouteNode = null;
                    curContainerDO.NextRouteNode = null;
                    uow.SaveChanges();
                }
            }
        }

        

        public async Task Execute(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            if (curContainerDO == null)
                throw new ArgumentNullException("ContainerDO is null");

            if (curContainerDO.CurrentRouteNode != null)
            {

                //break if CurrentActivity Is NULL, it means all activities 
                //are processed that there is no Next Activity to set as Current Activity
                do
                {
                    await _activity.Process(curContainerDO.CurrentRouteNode.Id, curContainerDO);
                    // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
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