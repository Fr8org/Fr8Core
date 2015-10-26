using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using StructureMap;

using Data.Interfaces.DataTransferObjects;
using Core.Managers;
using Data.Infrastructure;


namespace Core.Services
{
    public class Container : Core.Interfaces.IContainer
    {
        
        // Declarations

        private readonly EventReporter _alertReporter;
        private readonly IProcessNode _processNode;
        private readonly IRouteNode _activity;
        private readonly IRoute _route;

        
        

        public Container()
        {
            _alertReporter = ObjectFactory.GetInstance<EventReporter>();
            _processNode = ObjectFactory.GetInstance<IProcessNode>();
            _activity = ObjectFactory.GetInstance<IRouteNode>();
            _route = ObjectFactory.GetInstance<IRoute>();
        }

        

        /// <summary>
        /// New Process object
        /// </summary>
        /// <param name="processTemplateId"></param>
        /// <param name="envelopeId"></param>
        /// <returns></returns>
        public ContainerDO Create(IUnitOfWork uow, int processTemplateId, CrateDTO curEvent)
        {
            var containerDO = ObjectFactory.GetInstance<ContainerDO>();

                var curRoute = uow.RouteRepository.GetByKey(processTemplateId);
                if (curRoute == null)
                    throw new ArgumentNullException("processTemplateId");
                containerDO.Route = curRoute;

                containerDO.Name = curRoute.Name;
                containerDO.ContainerState = ContainerState.Unstarted;

                var crates = new List<CrateDTO>();
                if (curEvent != null)
                {
                    crates.Add(curEvent);
                }
                containerDO.UpdateCrateStorageDTO(crates);

            containerDO.CurrentRouteNode = _route.GetInitialActivity(uow, curRoute);

                uow.ContainerRepository.Add(containerDO);
                uow.SaveChanges();

                //then create process node
               var subrouteId = containerDO.Route.StartingSubroute.Id;

               var curProcessNode = _processNode.Create(uow, containerDO.Id, subrouteId, "process node");
               containerDO.ProcessNodes.Add(curProcessNode);
               EventManager.ContainerCreated(containerDO);
               uow.SaveChanges();

                

            return containerDO;
        }

        

        public async Task Launch(RouteDO curRoute, CrateDTO curEvent)
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
        public IList<ContainerDO> GetByFr8Account(IUnitOfWork unitOfWork, Fr8AccountDO account, bool isAdmin = false, int? id = null)
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