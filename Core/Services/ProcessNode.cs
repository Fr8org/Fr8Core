using System;
using System.Collections.Generic;
using System.Linq;
using Core.Helper;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Newtonsoft.Json;

namespace Core.Services
{
	public class ProcessNode: IProcessNodeService
	{
		/// <summary>
		/// Creates ProcessNode Object
		/// </summary>
		/// <returns>New ProcessNodeDO instance</returns>
		public ProcessNodeDO Create( IUnitOfWork uow, ProcessDO parentProcess )
		{
			var processNode = new ProcessNodeDO
			{
				ProcessNodeState = ProcessNodeState.Unstarted,
				//ProcessNodeTemplate = 
				ParentProcessId = parentProcess.Id
			};

			uow.ProcessNodeRepository.Add( processNode );

			return processNode;
		}

		/// <summary>
		/// Replaces the part of the TransitionKey's sourcePNode by the value of the targetPNode
		/// </summary>
		/// <param name="sourcePNode">ProcessNodeDO</param>
		/// <param name="targetPNode">ProcessNodeDO</param>
		public void CreateTruthTransition( ProcessNodeDO sourcePNode, ProcessNodeDO targetPNode )
		{
			var keys = JsonConvert.DeserializeObject< List< TransitionKeyData > >( sourcePNode.ProcessNodeTemplate.TransitionKey );
			
			foreach( var key in keys.Where( key => key.Flag.Equals( "false", StringComparison.OrdinalIgnoreCase ) ) )
			{
				key.Id = targetPNode.Id.ToString();
			}

			sourcePNode.ProcessNodeTemplate.TransitionKey = JsonConvert.SerializeObject( keys, Formatting.None );
		}
	}
}