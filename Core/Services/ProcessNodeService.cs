using System;
using System.Collections.Generic;
using System.Linq;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;

namespace Core.Services
{
	public class ProcessNodeService: IProcessNodeService
	{
		/// <summary>
		/// Creates ProcessNode Object
		/// </summary>
		/// <returns>New ProcessNodeDO instance</returns>
		public ProcessNodeDO Create( IUnitOfWork uow, ProcessDO parentProcess )
		{
			var processNode = new ProcessNodeDO
			{
				State = ProcessNodeDO.ProcessNodeState.Unstarted,
				ProcessNodeTemplate = parentProcess.StartingProcessNodeTemplate,
				ProcessID = parentProcess.Id
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
			var sourceKeys = sourcePNode.ProcessNodeTemplate.TransitionKey.Split( ',' );
			var targetKeys = targetPNode.ProcessNodeTemplate.TransitionKey.Split( ',' );

			this.ChangeSourceKeys( sourceKeys, targetKeys );
			this.ReplaceChangedValues( sourceKeys, sourcePNode );
		}

		private void ChangeSourceKeys( IList< string > sourceKeys, IList< string > targetKeys )
		{
			for( var i = 0; i < sourceKeys.Count; i++ )
			{
				var sourceId = sourceKeys[ i ].Split( ':' )[ 1 ];
				var targetId = targetKeys.FirstOrDefault( k => k.Split( ':' )[ 1 ].Equals( sourceId, StringComparison.OrdinalIgnoreCase ) );

				if( targetId != null )
				{
					sourceKeys[ i ] = sourceKeys[ i ].Replace( "false", "true" );
				}
			}
		}

		private void ReplaceChangedValues( IEnumerable< string > values, ProcessNodeDO sourcePNode )
		{
			sourcePNode.ProcessNodeTemplate.TransitionKey = string.Join( ",", values );
		}
	}
}