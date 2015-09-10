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
using Data.Wrappers;
using DocuSign.Integrations.Client;
using Utilities;

namespace Core.Services
{
    public class Process : IProcess
    {
        private readonly IProcessNode _processNode;
        private readonly DocuSignEnvelope _envelope;

        public Process()
        {
            _processNode = ObjectFactory.GetInstance<IProcessNode>();
            _envelope = new DocuSignEnvelope();
        }

        /// <summary>
        /// New Process object
        /// </summary>
        /// <param name="processTemplateId"></param>
        /// <param name="envelopeId"></param>
        /// <returns></returns>
        public ProcessDO Create(int processTemplateId, string envelopeId)
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
                curProcessDO.EnvelopeId = envelopeId;

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



        public void Launch(ProcessTemplateDO curProcessTemplate, DocuSignEventDO curEvent)
        {
            var curProcessDO = Create(curProcessTemplate.Id, curEvent.EnvelopeId);
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
                IActivity _activity = ObjectFactory.GetInstance<IActivity>();


                //break if CurrentActivity Is NULL, it means all activities 
                //are processed that there is no Next Activity to set as Current Activity
                do
                {
                    _activity.Process(curProcessDO.CurrentActivity);
                    SetProcessDOActivities(curProcessDO, _activity);
                } while (curProcessDO.CurrentActivity != null);
            }
            else
            {
                throw new ArgumentNullException("CurrentActivity is null. Cannot execute CurrentActivity");
            }
        }

        private void SetProcessDOActivities(ProcessDO curProcessDO, IActivity curActivity)
        {
            if (curProcessDO.NextActivity != null)
            {
                curProcessDO.CurrentActivity = curProcessDO.NextActivity;
            }
            else
            {
                //if ProcessDO.NextActivity is not set, 
                //get the downstream activities of the current activity and set the next current activity based on those downstream as CurrentActivity
                List<ActivityDO> activityLists = curActivity.GetDownstreamActivities(curProcessDO.CurrentActivity);
                if ((activityLists != null && activityLists.Count > 0) &&
                    curProcessDO.CurrentActivity.Id != activityLists[0].Id)//Needs to check if CurrentActivity is the same as the NextActivity which is a possible bug in DownStream
                {
                    curProcessDO.CurrentActivity = activityLists[0];
                }
                else
                    curProcessDO.CurrentActivity = null;
            }

            SetNextActivity(curProcessDO, curActivity);
        }

        private void SetNextActivity(ProcessDO curProcessDO, IActivity curActivity)
        {
            if (curProcessDO.CurrentActivity != null)
            {
                List<ActivityDO> activityLists = curActivity.GetDownstreamActivities(curProcessDO.CurrentActivity);
                if ((activityLists != null && activityLists.Count > 0) &&
                    curProcessDO.CurrentActivity.Id != activityLists[0].Id)//Needs to check if CurrentActivity is the same as the NextActivity which is a possible bug in DownStream
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