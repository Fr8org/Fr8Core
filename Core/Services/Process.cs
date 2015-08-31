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
        private readonly DocuSignEnvelope _envelope ;

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
                var curProcessNode = _processNode.Create(uow, curProcessDO.Id, processNodeTemplateId,"process node");
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
                var curProcessNode = uow.ProcessNodeRepository.GetByKey(curProcessDO.ProcessNodes.First().Id);
                curProcessDO.ProcessState = ProcessState.Executing;
                uow.SaveChanges();

                Execute(curEvent, curProcessNode);
            }
        }

        public void Execute(DocuSignEventDO curEvent, ProcessNodeDO curProcessNode)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curDocuSignEnvelope = new DocuSignEnvelope(); //should just change GetEnvelopeData to pass an EnvelopeDO
                List<EnvelopeDataDTO> curEnvelopeData = _envelope.GetEnvelopeData(curDocuSignEnvelope);

                while (curProcessNode != null)
                {
                    string nodeExecutionResultKey = _processNode.Execute(curEnvelopeData, curProcessNode);
                    if (nodeExecutionResultKey != string.Empty)
                    {
                        var nodeTransitions = JsonConvert.DeserializeObject<List<TransitionKeyData>>(curProcessNode.ProcessNodeTemplate.NodeTransitions);
                        int nodeID = nodeTransitions.Where(k => k.Flag.Equals(nodeExecutionResultKey, StringComparison.InvariantCultureIgnoreCase)).DefaultIfEmpty(new TransitionKeyData()).FirstOrDefault().Id;
                        if (nodeTransitions != null && nodeID != 0)
                        {
                            curProcessNode = uow.ProcessNodeRepository.GetByKey(nodeID);
                        }
                        else
                        {
                            throw new Exception("ProcessNode.NodeTransitions did not have a key matching the returned transition target from Critera");
                        }
                    }
                }
            }
        }
    }
}