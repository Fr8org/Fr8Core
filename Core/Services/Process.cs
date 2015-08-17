using System;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using StructureMap;

namespace Core.Services
{
    public class Process : IProcess
    {
        private readonly IProcessNode _processNode;

        public Process()
        {
            _processNode = ObjectFactory.GetInstance<IProcessNode>();
        }

        /// <summary>
        /// New Process object
        /// </summary>
        /// <param name="processTemplateId"></param>
        /// <param name="envelopeId"></param>
        /// <returns></returns>
        public ProcessDO Create(int processTemplateId, int envelopeId)
        {
            var curProcessDO = ObjectFactory.GetInstance<ProcessDO>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var template = uow.ProcessTemplateRepository.GetByKey(processTemplateId);
                var envelope = uow.EnvelopeRepository.GetByKey(envelopeId);

                if (template == null)
                    throw new ArgumentNullException("processTemplateId");
                if (envelope == null)
                    throw new ArgumentNullException("envelopeId");

                curProcessDO.Name = template.Name;
                curProcessDO.ProcessState = ProcessState.Unstarted;
                curProcessDO.EnvelopeId = envelopeId.ToString();

                var processNode = _processNode.Create(uow, curProcessDO, "process node");
                uow.SaveChanges();

                curProcessDO.CurrentProcessNodeId = processNode.Id;

                uow.ProcessRepository.Add(curProcessDO);
                uow.SaveChanges();
            }
            return curProcessDO;
        }

        public void Launch(ProcessTemplateDO curProcessTemplate, EnvelopeDO curEnvelope)
        {
            var curProcessDO = Create(curProcessTemplate.Id, curEnvelope.Id);
            if (curProcessDO.ProcessState == ProcessState.Failed || curProcessDO.ProcessState == ProcessState.Completed)
                throw new ApplicationException("Attempted to Launch a Process that was Failed or Completed");

            curProcessDO.ProcessState = ProcessState.Executing;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                ProcessNodeDO curProcessNode;
                if (curProcessDO.CurrentProcessNodeId == 0)
                {
                    curProcessNode = _processNode.Create(uow, curProcessDO, "process node");
                    uow.SaveChanges();
                }
                curProcessNode = uow.ProcessNodeRepository.GetByKey(curProcessDO.CurrentProcessNodeId);

                _processNode.Execute(curEnvelope, curProcessNode);
            }
        }
    }
}