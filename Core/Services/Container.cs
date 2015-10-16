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


namespace Core.Services
{
    public class Container : Core.Interfaces.IContainer
    {
        
        // Declarations
        

        private readonly IProcessNode _processNode;
        private readonly IRouteNode _activity;
        private readonly IRoute _route;

        
        

        public Container()
        {
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

                uow.SaveChanges();

            return containerDO;
        }

        

        public async Task Launch(RouteDO curRoute, CrateDTO curEvent)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curProcessDO = Create(uow, curRoute.Id, curEvent);

            if (curProcessDO.ContainerState == ContainerState.Failed || curProcessDO.ContainerState == ContainerState.Completed)
                {
                throw new ApplicationException("Attempted to Launch a Process that was Failed or Completed");
                }

                curProcessDO.ContainerState = ContainerState.Executing;
                uow.SaveChanges();

                try
                {
                    await Execute(uow, curProcessDO);
                    curProcessDO.ContainerState = ContainerState.Completed;
            }
                catch
                {
                    curProcessDO.ContainerState = ContainerState.Failed;
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
                throw new ArgumentNullException("ProcessDO is null");

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

        // Return the Processes of current Account
        public IList<ContainerDO> GetByDockyardAccount(string userId, bool isAdmin = false, int? id = null)
        {
            if (userId == null)
              throw new ApplicationException("UserId must not be null");
            
            using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var containerRepository = unitOfWork.ContainerRepository.GetQuery();

                if (isAdmin)
                {
                    return  (id == null
                   ? containerRepository
                   : containerRepository.Where(container => container.Id == id)).ToList();
                }

                return  (id == null
                   ? containerRepository.Where(container => container.Route.Fr8Account.Id == userId)
                   : containerRepository.Where(container => container.Id == id && container.Route.Fr8Account.Id == userId)).ToList();
            }
        }

             
    }
}