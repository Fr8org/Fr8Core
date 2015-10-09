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
        /**********************************************************************************/
        // Declarations
        /**********************************************************************************/

        private readonly IProcessNode _processNode;
        private readonly IActivity _activity;
        private readonly IProcessTemplate _processTemplate;

        /**********************************************************************************/
        // Functions
        /**********************************************************************************/

        public Process()
        {
            _processNode = ObjectFactory.GetInstance<IProcessNode>();
            _activity = ObjectFactory.GetInstance<IActivity>();
            _processTemplate = ObjectFactory.GetInstance<IProcessTemplate>();
        }

        /**********************************************************************************/
        /// <summary>
        /// New Process object
        /// </summary>
        /// <param name="processTemplateId"></param>
        /// <param name="envelopeId"></param>
        /// <returns></returns>
        public ProcessDO Create(int processTemplateId, CrateDTO curEvent)
        {
            var curProcessDO = ObjectFactory.GetInstance<ProcessDO>();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curProcessTemplate = uow.ProcessTemplateRepository.GetByKey(processTemplateId);
                if (curProcessTemplate == null)
                    throw new ArgumentNullException("processTemplateId");
                curProcessDO.ProcessTemplate = curProcessTemplate;

                curProcessDO.Name = curProcessTemplate.Name;
                curProcessDO.ProcessState = ProcessState.Unstarted;
                curProcessDO.UpdateCrateStorageDTO(new List<CrateDTO> {curEvent});

                curProcessDO.CurrentActivity = _processTemplate.GetInitialActivity(uow, curProcessTemplate);

                uow.ProcessRepository.Add(curProcessDO);
                uow.SaveChanges();

                //then create process node
                var processNodeTemplateId = curProcessDO.ProcessTemplate.StartingProcessNodeTemplate.Id;
                
                var curProcessNode = _processNode.Create(uow,curProcessDO.Id, processNodeTemplateId,"process node");
                curProcessDO.ProcessNodes.Add(curProcessNode);

                uow.SaveChanges();
            }
            return curProcessDO;
        }

        /**********************************************************************************/

        public async Task Launch(ProcessTemplateDO curProcessTemplate, CrateDTO curEvent)
        {
            var curProcessDO = Create(curProcessTemplate.Id, curEvent);

            if (curProcessDO.ProcessState == ProcessState.Failed || curProcessDO.ProcessState == ProcessState.Completed)
            {
                throw new ApplicationException("Attempted to Launch a Process that was Failed or Completed");
            }

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                curProcessDO.ProcessState = ProcessState.Executing;
                uow.SaveChanges();

                try
                {
                await Execute(curProcessDO);
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

        /**********************************************************************************/

        public async Task Execute(ProcessDO curProcessDO)
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
                } while (MoveToTheNextActivity(curProcessDO) != null);
            }
            else
            {
                throw new ArgumentNullException("CurrentActivity is null. Cannot execute CurrentActivity");
            }
        }

        /**********************************************************************************/

        private ActivityDO MoveToTheNextActivity(ProcessDO process)
        {
            // we relay on the fact that processtemplate has only one process node template. In future it will be fixed.
            if (process.ProcessTemplate.ProcessNodeTemplates.Count == 0)
            {
                throw new Exception("ProcessTemplate has no ProcessNodeTemplates");
            }

            if (process.ProcessTemplate.ProcessNodeTemplates.Count > 1)
                {
                throw new Exception("ProcessTemplate has multiple ProcessNodeTemplates");
            }

            process.CurrentActivity = process.ProcessTemplate.ProcessNodeTemplates[0].Actions
                                              .OrderBy(x => x.Ordering)
                                              .FirstOrDefault(x => x.Ordering > process.CurrentActivity.Ordering);
            return process.CurrentActivity;
        }

        /**********************************************************************************/

    }
}