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

                //create process
                uow.ProcessRepository.Add(curProcessDO);
                uow.SaveChanges();

                //then create process node
                var curProcessNode = _processNode.Create(uow, curProcessDO, "process node");
                curProcessDO.ProcessNodes.Add(curProcessNode);
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
                ProcessNodeDO curProcessNode = uow.ProcessNodeRepository.GetByKey(curProcessDO.ProcessNodes.First().Id);
                Execute(curEnvelope, curProcessNode);
            }
        }

        public void Execute(EnvelopeDO curEnvelope, ProcessNodeDO curProcessNode)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                while (curProcessNode != null)
                {
                    string nodeTransitionKey = _processNode.Execute(curEnvelope, curProcessNode);
                    if (nodeTransitionKey != string.Empty)
                    {
                        var nodeTransitions = JsonConvert.DeserializeObject<List<TransitionKeyData>>(curProcessNode.ProcessNodeTemplate.NodeTransitions);
                        string nodeID = nodeTransitions.Where(k => k.Flag.Equals(nodeTransitionKey, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault().Id;
                        if (nodeTransitions != null && String.IsNullOrEmpty(nodeID) != true)
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