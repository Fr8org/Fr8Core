using System;
using System.Collections.Generic;
using System.Linq;
using Data.Constants;
using Newtonsoft.Json;
using StructureMap;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hub.Interfaces;
using Hub.Helper;

namespace Hub.Services
{
    public class ProcessNode : IProcessNode
    {
        
        // Declarations
        

        private readonly ICriteria _criteria;

        
        

        public ProcessNode()
        {
            _criteria = ObjectFactory.GetInstance<ICriteria>();
        }

        
        /// <summary>
        /// Creates ProcessNode Object
        /// </summary>
        /// <returns>New ProcessNodeDO instance</returns>
        public ProcessNodeDO Create(IUnitOfWork uow, Guid parentContainerId,
            Guid subrouteId, string name = "ProcessNode")
        {
            var processNode = new ProcessNodeDO
            {
                ProcessNodeState = ProcessNodeState.Unstarted,
                Name = name,
                ParentContainerId = parentContainerId
            };

            processNode.SubrouteId = subrouteId;

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
                JsonConvert.DeserializeObject<List<ProcessNodeTransition>>(sourcePNode.Subroute.NodeTransitions);

            if (!this.IsCorrectKeysCountValid(keys))
                throw new ArgumentException("There should only be one key with false.");

            var key = keys.First(k => k.TransitionKey.Equals("false", StringComparison.OrdinalIgnoreCase));
            key.ProcessNodeId = targetPNode.Id.ToString();

            sourcePNode.Subroute.NodeTransitions = JsonConvert.SerializeObject(keys, Formatting.None);
        }

        
        //Obsolete
        //public string Execute(List<EnvelopeDataDTO> curEventData, ProcessNodeDO curProcessNode)
        //{
        //    string nextTransitionKey;

        //    var result = _criteria.Evaluate(curEventData, curProcessNode);
        //    if (result)
        //    {
        //        var activityService = ObjectFactory.GetInstance<IRouteNode>();

        //        using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //        {
        //            var curSubroute = uow.PlanRepository.GetById<SubrouteDO>(curProcessNode.SubrouteId);
        //            RouteNodeDO currentAction = curSubroute;

        //            do
        //            {
        //                activityService.Process(currentAction.Id, ActivityState.InitialRun, curProcessNode.ParentContainer);
        //                currentAction = activityService.GetNextActivity(currentAction, curSubroute);
        //            } while (currentAction != null);
        //        }

        //        nextTransitionKey = "true";
        //    }
        //    else
        //    {
        //        nextTransitionKey = "false";
        //    }

        //    return nextTransitionKey;
        //}

        
        /// <summary>
        /// There will and should only be one key with false. if there's more than one, throw an exception.	
        /// </summary>
        /// <param name="keys">keys to be validated</param>
        private bool IsCorrectKeysCountValid(IEnumerable<ProcessNodeTransition> keys)
        {
            var count = keys.Count(key => key.TransitionKey.Equals("false", StringComparison.OrdinalIgnoreCase));
            return count == 1;
        }

        
    }
}