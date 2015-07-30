using System.Linq;
using Data.Interfaces;
using Data.States;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Entities
{
	[ TestFixture ]
	public class ProcessNodeTests: BaseTest
	{
		private FixtureData _fixture;
		private IUnitOfWork _uow;

		[ SetUp ]
		public void Setup()
		{
			this._uow = ObjectFactory.GetInstance< IUnitOfWork >();
			this._fixture = new FixtureData( this._uow );
		}

		[ Test ]
		[ Category( "ProcessNode" ) ]
		public void ProcessNode_CanCreateUpdateChangeStatus()
		{
			const int newStatus = ProcessNodeState.Unstarted;
			const int updatedStatus = ProcessNodeState.Complete;
			using( var uow = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				uow.ProcessNodeRepository.Add( this._fixture.TestProcessNode() );
				uow.SaveChanges();

				var createdNode = uow.ProcessNodeRepository.GetQuery().FirstOrDefault();
				Assert.NotNull( createdNode );
				Assert.AreEqual( newStatus, createdNode.ProcessNodeState );

				createdNode.ProcessNodeState = updatedStatus;
				uow.SaveChanges();

				var updatedNode = uow.ProcessNodeRepository.GetQuery().FirstOrDefault();
				Assert.NotNull( updatedNode );
				Assert.AreEqual( updatedStatus, updatedNode.ProcessNodeState );
			}
		}
	}
}