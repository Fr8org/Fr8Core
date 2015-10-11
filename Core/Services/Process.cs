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
        private readonly IProcessNode _processNode;
        private readonly IActivity _activity;
        private readonly IProcessTemplate _processTemplate;

        public Process()
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

                var crates = new List<CrateDTO>();
                if (curEvent != null)
                {
                    crates.Add(curEvent);
                }
                curProcessDO.UpdateCrateStorageDTO(crates);

                curProcessDO.CurrentActivity = _processTemplate
                    .GetInitialActivity(uow, curProcessTemplate);

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

        public async Task Launch(ProcessTemplateDO curProcessTemplate, CrateDTO curEvent)
        {
            var curProcessDO = Create(curProcessTemplate.Id, curEvent);
            if (curProcessDO.ProcessState == ProcessState.Failed || curProcessDO.ProcessState == ProcessState.Completed)
                throw new ApplicationException("Attempted to Launch a Process that was Failed or Completed");

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                curProcessDO.ProcessState = ProcessState.Executing;
                uow.SaveChanges();

                await Execute(curProcessDO);
            }
        }

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
                    UpdateNextActivity(curProcessDO);
                } while (curProcessDO.CurrentActivity != null);
            }
            else
            {
                throw new ArgumentNullException("CurrentActivity is null. Cannot execute CurrentActivity");
            }
        }

        private void UpdateNextActivity(ProcessDO curProcessDO)
        {
            if (curProcessDO == null)
                throw new ArgumentNullException("ProcessDO is null");

            if (curProcessDO.NextActivity != null)
            {
                curProcessDO.CurrentActivity = curProcessDO.NextActivity;
            }
            else
            {
                //if ProcessDO.NextActivity is not set, 
                //get the downstream activities of the current activity and set the next current activity based on those downstream as CurrentActivity
                List<ActivityDO> activityLists = _activity.GetNextActivities(curProcessDO.CurrentActivity).ToList();
                if ((activityLists != null && activityLists.Count > 0) &&
                    curProcessDO.CurrentActivity.Id != activityLists[0].Id)//Needs to check if CurrentActivity is the same as the NextActivity which is a possible bug in GetNextActivities and possible infinite loop
                {
                    curProcessDO.CurrentActivity = activityLists[0];
                }
                else
                    curProcessDO.CurrentActivity = null;
            }

            SetProcessNextActivity(curProcessDO);
        }

        public void SetProcessNextActivity(ProcessDO curProcessDO)
        {
            if(curProcessDO == null)
                throw new ArgumentNullException("Paramter ProcessDO is null.");

            if (curProcessDO.CurrentActivity != null)
            {
                List<ActivityDO> activityLists = _activity.GetNextActivities(curProcessDO.CurrentActivity).ToList();
                if ((activityLists != null && activityLists.Count > 0) &&
                    curProcessDO.CurrentActivity.Id != activityLists[0].Id)//Needs to check if CurrentActivity is the same as the NextActivity which is a possible bug in GetNextActivities and possible infinite loop
                {
                   curProcessDO.NextActivity = activityLists[0];
                }
                else
                {
                    curProcessDO.NextActivity = null;
                }
            }
            else
            {
                curProcessDO.NextActivity = null;//set NexActivity to null since the currentActivity is null
            }
        }

    }
}