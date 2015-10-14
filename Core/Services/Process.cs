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
    public class Process : IProcess
    {
        
        // Declarations
        

        private readonly IProcessNode _processNode;
        private readonly IActivity _activity;
        private readonly IRoute _route;

        
        

        public Process()
        {
            _processNode = ObjectFactory.GetInstance<IProcessNode>();
            _activity = ObjectFactory.GetInstance<IActivity>();
            _route = ObjectFactory.GetInstance<IRoute>();
        }

        

        /// <summary>
        /// New Process object
        /// </summary>
        /// <param name="processTemplateId"></param>
        /// <param name="envelopeId"></param>
        /// <returns></returns>
        public ProcessDO Create(IUnitOfWork uow, int processTemplateId, CrateDTO curEvent)
        {
            var curProcessDO = ObjectFactory.GetInstance<ProcessDO>();

                var curRoute = uow.RouteRepository.GetByKey(processTemplateId);
                if (curRoute == null)
                    throw new ArgumentNullException("processTemplateId");
                curProcessDO.Route = curRoute;

                curProcessDO.Name = curRoute.Name;
                curProcessDO.ProcessState = ProcessState.Unstarted;

                var crates = new List<CrateDTO>();
                if (curEvent != null)
                {
                    crates.Add(curEvent);
                }
                curProcessDO.UpdateCrateStorageDTO(crates);

            curProcessDO.CurrentActivity = _route.GetInitialActivity(uow, curRoute);

                uow.ProcessRepository.Add(curProcessDO);
                uow.SaveChanges();

                //then create process node
                var processNodeTemplateId = curProcessDO.Route.StartingProcessNodeTemplate.Id;
                
            var curProcessNode = _processNode.Create(uow, curProcessDO.Id, processNodeTemplateId, "process node");
                curProcessDO.ProcessNodes.Add(curProcessNode);

                uow.SaveChanges();

            return curProcessDO;
        }

        

        public async Task Launch(RouteDO curRoute, CrateDTO curEvent)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curProcessDO = Create(uow, curRoute.Id, curEvent);

            if (curProcessDO.ProcessState == ProcessState.Failed || curProcessDO.ProcessState == ProcessState.Completed)
                {
                throw new ApplicationException("Attempted to Launch a Process that was Failed or Completed");
                }

                curProcessDO.ProcessState = ProcessState.Executing;
                uow.SaveChanges();

                try
                {
                    await Execute(uow, curProcessDO);
                    curProcessDO.ProcessState = ProcessState.Completed;
            }
                catch
                {
                    curProcessDO.ProcessState = ProcessState.Failed;
                    throw;
        }
                finally
                {
                    uow.SaveChanges();
                }
            }
        }

        

        public async Task Execute(IUnitOfWork uow, ProcessDO curProcessDO)
        {
            if (curProcessDO == null)
                throw new ArgumentNullException("ProcessDO is null");

            if (curProcessDO.CurrentActivity != null)
            {

                //break if CurrentActivity Is NULL, it means all activities 
                //are processed that there is no Next Activity to set as Current Activity
                do
                {
                    await _activity.Process(curProcessDO.CurrentActivity.Id, curProcessDO);
                    // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
                } while (MoveToTheNextActivity(uow, curProcessDO) != null);
            }
            else
            {
                throw new ArgumentNullException("CurrentActivity is null. Cannot execute CurrentActivity");
            }
        }

        

        private ActivityDO MoveToTheNextActivity(IUnitOfWork uow, ProcessDO curProcessDo)
        {
            var next =  ObjectFactory.GetInstance<IActivity>().GetNextActivity(curProcessDo.CurrentActivity, null);

            // very simple check for cycles
            if (next != null && next == curProcessDo.CurrentActivity)
                {
                throw new Exception(string.Format("Cycle detected. Current activty is {0}", curProcessDo.CurrentActivity.Id));
            }

            curProcessDo.CurrentActivity = next;

            uow.SaveChanges();

            return next;
        }

        

    }
}