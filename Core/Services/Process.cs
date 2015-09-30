using System;
using System.Data.Entity;
using System.Linq;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using StructureMap;
using Newtonsoft.Json;
using Core.Helper;
using System.Collections.Generic;
using System.Linq;
using Data.Interfaces.DataTransferObjects;
using Data.States.Templates;
using Data.Wrappers;
using DocuSign.Integrations.Client;
using Utilities;

namespace Core.Services
{
    public class Process : IProcess
    {
        private readonly IProcessNode _processNode;
        private readonly DocuSignEnvelope _envelope;
        private readonly IActivity _activity;
        private readonly IProcessTemplate _processTemplate;

        public Process()
        {
            _processNode = ObjectFactory.GetInstance<IProcessNode>();
            _envelope = new DocuSignEnvelope();
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
                curProcessDO.UpdateCrateStorageDTO(new List<CrateDTO>() { curEvent });

                curProcessDO.CurrentActivity = _processTemplate.GetInitialActivity(curProcessTemplate);

                uow.ProcessRepository.Add(curProcessDO);
                uow.SaveChanges();

                //then create process node
                var processNodeTemplateId = curProcessDO.ProcessTemplate.StartingProcessNodeTemplateId;
                var curProcessNode = _processNode.Create(uow,curProcessDO.Id, processNodeTemplateId,"process node");
                curProcessDO.ProcessNodes.Add(curProcessNode);

                uow.SaveChanges();
            }
            return curProcessDO;
        }





        public void Launch(ProcessTemplateDO curProcessTemplate, CrateDTO curEvent)
        {
            var curProcessDO = Create(curProcessTemplate.Id, curEvent);
            if (curProcessDO.ProcessState == ProcessState.Failed || curProcessDO.ProcessState == ProcessState.Completed)
                throw new ApplicationException("Attempted to Launch a Process that was Failed or Completed");

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                curProcessDO.ProcessState = ProcessState.Executing;
                uow.SaveChanges();

                Execute(curProcessDO);
            }
        }

        public void Execute(ProcessDO curProcessDO)
        {
            if (curProcessDO == null)
                throw new ArgumentNullException("ProcessDO is null");

            if (curProcessDO.CurrentActivity != null)
            {

                //break if CurrentActivity Is NULL, it means all activities 
                //are processed that there is no Next Activity to set as Current Activity
                do
                {
                    _activity.Process(curProcessDO.CurrentActivity.Id, curProcessDO);
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

        private void SetProcessNextActivity(ProcessDO curProcessDO)
        {
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
        }

    }
}