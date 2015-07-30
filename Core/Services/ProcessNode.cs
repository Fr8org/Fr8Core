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

			if( !this.IsCorrectKeysCountValid( keys ) )
				throw new ArgumentException( "There should only be one key with false." );

			var key = keys.First( k => k.Flag.Equals( "false", StringComparison.OrdinalIgnoreCase ) );
			key.Id = targetPNode.Id.ToString();

			sourcePNode.ProcessNodeTemplate.TransitionKey = JsonConvert.SerializeObject( keys, Formatting.None );
		}

		/// <summary>
		/// There will and should only be one key with false. if there's more than one, throw an exception.	
		/// </summary>
		/// <param name="keys">keys to be validated</param>
		private bool IsCorrectKeysCountValid( IEnumerable< TransitionKeyData > keys )
		{
			var count = keys.Count( key => key.Flag.Equals( "false", StringComparison.OrdinalIgnoreCase ) );
			return count == 1;
		}
	}
}