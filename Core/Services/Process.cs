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
            var curProcess = new ProcessDO();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var template = uow.ProcessTemplateRepository.GetByKey(processTemplateId);
                var envelope = uow.EnvelopeRepository.GetByKey(envelopeId);

                if (template == null)
                    throw new ArgumentNullException("processTemplateId");
                if (envelope == null)
                    throw new ArgumentNullException("envelopeId");

                curProcess.Name = template.Name;
                curProcess.ProcessState = ProcessState.Executing;
                curProcess.EnvelopeId = envelopeId.ToString();

                var processNode = _processNode.Create(uow, curProcess);
                uow.SaveChanges();

                curProcess.CurrentProcessNodeId = processNode.Id;

                uow.ProcessRepository.Add(curProcess);
                uow.SaveChanges();
            }
            return curProcess;
        }

        public void Execute(ProcessTemplateDO curProcessTemplate, EnvelopeDO curEnvelope)
        {
            var curProcessDo = Create(curProcessTemplate.Id, curEnvelope.Id);
            if (curProcessDo.ProcessState == ProcessState.Failed || curProcessDo.ProcessState == ProcessState.Completed)
                return;

            curProcessDo.ProcessState = ProcessState.Executing;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                ProcessNodeDO curProcessNode;
                if (curProcessDo.CurrentProcessNodeId == 0)
                {
                    var curProcessNodeTemplate =
                        uow.ProcessNodeTemplateRepository.GetByKey(curProcessTemplate.StartingProcessNodeTemplate);
                    curProcessNode = new ProcessNodeDO();
                    curProcessNode.Name = curProcessNodeTemplate.Name;
                    curProcessNode.ParentProcessId = curProcessDo.Id;
                    uow.SaveChanges();
                }
                curProcessNode = uow.ProcessNodeRepository.GetByKey(curProcessDo.CurrentProcessNodeId);

                _processNode.Execute(curProcessDo, curEnvelope, curProcessNode);
            }
        }
    }
}