using System.Linq;
using Data.Entities;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;

namespace DockyardTest.Entities
{
	[ TestFixture ]
	public class ProcessNodeTests: BaseTest
	{
		[ Test ]
		[ Category( "ProcessNode" ) ]
		public void ProcessNode_Change_Status()
		{
			const ProcessNodeDO.ProcessNodeState newSate = ProcessNodeDO.ProcessNodeState.Unstarted;
			const ProcessNodeDO.ProcessNodeState updatedState = ProcessNodeDO.ProcessNodeState.Complete;
			using( var uow = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				uow.ProcessNodeRepository.Add( new ProcessNodeDO
				{
					Id = 1,
					State = newSate
				} );
				uow.SaveChanges();

				var createdNode = uow.ProcessNodeRepository.GetQuery().FirstOrDefault();
				Assert.NotNull( createdNode );
				Assert.AreEqual( newSate, createdNode.State );

				createdNode.State = updatedState;
				uow.SaveChanges();

				var updatedNode = uow.ProcessNodeRepository.GetQuery().FirstOrDefault();
				Assert.NotNull( updatedNode );
				Assert.AreEqual( updatedState, updatedNode.State );
			}
		}
	}
}