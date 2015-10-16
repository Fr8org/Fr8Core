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
        private readonly IActivity _activity;
        private readonly IProcessTemplate _processTemplate;

        
        

        public Container()
        {
            _processNode = ObjectFactory.GetInstance<IProcessNode>();
            _activity = ObjectFactory.GetInstance<IActivity>();
            _processTemplate = ObjectFactory.GetInstance<IProcessTemplate>();
        }

        

        /// <summary>
        /// New Process object
        /// </summary>
        /// <param name="processTemplateId"></param>
        /// <param name="envelopeId"></param>
        /// <returns></returns>
        public ContainerDO Create(IUnitOfWork uow, int processTemplateId, CrateDTO curEvent)
        {
            var curProcessDO = ObjectFactory.GetInstance<ContainerDO>();

                var curProcessTemplate = uow.ProcessTemplateRepository.GetByKey(processTemplateId);
                if (curProcessTemplate == null)
                    throw new ArgumentNullException("processTemplateId");
                curProcessDO.ProcessTemplate = curProcessTemplate;

                curProcessDO.Name = curProcessTemplate.Name;
                curProcessDO.ContainerState = ContainerState.Unstarted;

                var crates = new List<CrateDTO>();
                if (curEvent != null)
                {
                    crates.Add(curEvent);
                }
                curProcessDO.UpdateCrateStorageDTO(crates);

            curProcessDO.CurrentActivity = _processTemplate.GetInitialActivity(uow, curProcessTemplate);

                uow.ContainerRepository.Add(curProcessDO);
                uow.SaveChanges();

                //then create process node
                var processNodeTemplateId = curProcessDO.ProcessTemplate.StartingProcessNodeTemplate.Id;
                
            var curProcessNode = _processNode.Create(uow, curProcessDO.Id, processNodeTemplateId, "process node");
                curProcessDO.ProcessNodes.Add(curProcessNode);

                uow.SaveChanges();

            return curProcessDO;
        }

        

        public async Task Launch(ProcessTemplateDO curProcessTemplate, CrateDTO curEvent)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curProcessDO = Create(uow, curProcessTemplate.Id, curEvent);

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

            if (curContainerDO.CurrentActivity != null)
            {

                //break if CurrentActivity Is NULL, it means all activities 
                //are processed that there is no Next Activity to set as Current Activity
                do
                {
                    await _activity.Process(curContainerDO.CurrentActivity.Id, curContainerDO);
                    // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
                } while (MoveToTheNextActivity(uow, curContainerDO) != null);
            }
            else
            {
                throw new ArgumentNullException("CurrentActivity is null. Cannot execute CurrentActivity");
            }
        }

        

        private ActivityDO MoveToTheNextActivity(IUnitOfWork uow, ContainerDO curContainerDo)
        {
            var next =  ObjectFactory.GetInstance<IActivity>().GetNextActivity(curContainerDo.CurrentActivity, null);

            // very simple check for cycles
            if (next != null && next == curContainerDo.CurrentActivity)
                {
                throw new Exception(string.Format("Cycle detected. Current activty is {0}", curContainerDo.CurrentActivity.Id));
            }

            curContainerDo.CurrentActivity = next;

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
                   ? containerRepository.Where(container => container.ProcessTemplate.DockyardAccount.Id == userId)
                   : containerRepository.Where(container => container.Id == id && container.ProcessTemplate.DockyardAccount.Id == userId)).ToList();
            }
        }

             
    }
}