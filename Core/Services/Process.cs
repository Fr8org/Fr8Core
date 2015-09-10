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

                Execute(curEvent, curProcessDO);
            }
        }

        public void Execute(DocuSignEventDO curEvent, ProcessDO curProcessDO)
        {
            if (curProcessDO == null)
                throw new ArgumentNullException("ProcessDO is null");
            if (curEvent == null)
                throw new ArgumentNullException("DocuSignEventDO is null");

            if (curProcessDO.CurrentActivity != null)
            {
                IActivity _activity = ObjectFactory.GetInstance<IActivity>();


                //Process the CurrentActivity after processing move the NextActivity as the CurrentActivity
                //loop until the NextActivity is null AND CurrentActivity is the same as before (mean it didnt moved to the nextactivity)

                ActivityDO tmpCurrentActivity = curProcessDO.CurrentActivity;
                do
                {
                    tmpCurrentActivity = curProcessDO.CurrentActivity;//store current activity to be evaluated if the originated current activity is moved to next activity
                    _activity.Process(curProcessDO.CurrentActivity);
                    SetProcessDOActivities(curProcessDO, _activity);
                } while (curProcessDO.NextActivity != null && !tmpCurrentActivity.Equals(curProcessDO.CurrentActivity));
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
                if (activityLists.Count > 0)
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
                if (activityLists.Count > 0)
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