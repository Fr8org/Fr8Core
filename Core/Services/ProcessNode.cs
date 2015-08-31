using System;
using System.Collections.Generic;
using System.Linq;
using Core.Helper;
using Core.Interfaces;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Repositories;
using Data.States;
using Newtonsoft.Json;
using StructureMap;

namespace Core.Services
{
    public class ProcessNode : IProcessNode
    {
        private IProcessNodeTemplateRepository _processNodeTemplateRepository;

        public ProcessNode()
        {
        }

        /// <summary>
        /// Creates ProcessNode Object
        /// </summary>
        /// <returns>New ProcessNodeDO instance</returns>
        public ProcessNodeDO Create(IUnitOfWork uow, int parentProcessId, int processNodeTemplateId, string name="ProcessNode")
        {
            var processNode = new ProcessNodeDO
            {
                ProcessNodeState = ProcessNodeState.Unstarted,
                Name = name,
                ParentProcessId = parentProcessId
            };

            processNode.ProcessNodeTemplateId = processNodeTemplateId;
            processNode.ProcessNodeTemplate = uow.ProcessNodeTemplateRepository.GetByKey(processNodeTemplateId);

            uow.ProcessNodeRepository.Add(processNode);
            EventManager.ProcessNodeCreated(processNode);

            return processNode;
        }

        /// <summary>
        /// Replaces the part of the TransitionKey's sourcePNode by the value of the targetPNode
        /// </summary>
        /// <param name="sourcePNode">ProcessNodeDO</param>
        /// <param name="targetPNode">ProcessNodeDO</param>
        public void CreateTruthTransition(ProcessNodeDO sourcePNode, ProcessNodeDO targetPNode)
        {
            var keys =
                JsonConvert.DeserializeObject<List<TransitionKeyData>>(sourcePNode.ProcessNodeTemplate.NodeTransitions);

            if (!this.IsCorrectKeysCountValid(keys))
                throw new ArgumentException("There should only be one key with false.");

            var key = keys.First(k => k.Flag.Equals("false", StringComparison.OrdinalIgnoreCase));
            key.Id = targetPNode.Id;

            sourcePNode.ProcessNodeTemplate.NodeTransitions = JsonConvert.SerializeObject(keys, Formatting.None);
        }

        public string Execute(List<EnvelopeDataDTO> curEventData, ProcessNodeDO curProcessNode)
        {
            var _criteria = ObjectFactory.GetInstance<ICriteria>();
            var result = _criteria.Evaluate(curEventData, curProcessNode);
            if (result)
            {
                var _curActionList = ObjectFactory.GetInstance<IActionList>();
                List<ActionListDO> actionListSet = curProcessNode.ProcessNodeTemplate.ActionLists.Where(t => t.ActionListType == ActionListType.Immediate).ToList(); //this will break when we add additional ActionLists, and will need attention
                foreach (var actionList in actionListSet)
                {
                    _curActionList.Process(actionList);
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// There will and should only be one key with false. if there's more than one, throw an exception.	
        /// </summary>
        /// <param name="keys">keys to be validated</param>
        private bool IsCorrectKeysCountValid(IEnumerable<TransitionKeyData> keys)
        {
            var count = keys.Count(key => key.Flag.Equals("false", StringComparison.OrdinalIgnoreCase));
            return count == 1;
        }
    }
}