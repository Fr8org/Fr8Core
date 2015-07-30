using System;
using System.Linq;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Services
{
	[ TestFixture ]
	[ Category( "ProcessNodeService" ) ]
	public class ProcessNodeServiceTests: BaseTest
	{
		private IProcessNodeService _service;

		[ SetUp ]
		public override void SetUp()
		{
			base.SetUp();
			this._service = ObjectFactory.GetInstance< IProcessNodeService >();
		}

		[ Test ]
		public void ProcessService_Can_CreateProcessNode()
		{
			using( var uow = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				var process = FixtureData.GetProcesses().First();
				var processNode = this._service.Create( uow, process );

				Assert.AreEqual( processNode.ParentProcessId, process.Id );
			}
		}

		[ Test ]
		public void ProcessNodeService_Can_CreateTruthTransition()
		{
			const string sourceIds = "[{\"Flag\":\"true\",\"Id\":\"234kljdf\"},{\"Flag\":\"false\",\"Id\":\"dfgkjfg\"}]";
			const string correctlyChangedIds = "[{\"Flag\":\"true\",\"Id\":\"234kljdf\"},{\"Flag\":\"false\",\"Id\":\"223\"}]";

			var sourceNode = new ProcessNodeDO { ProcessNodeTemplate = new ProcessNodeTemplateDO { TransitionKey = sourceIds } };
			var targetNode = new ProcessNodeDO { Id = 223 };

			this._service.CreateTruthTransition( sourceNode, targetNode );

			Assert.IsTrue( sourceNode.ProcessNodeTemplate.TransitionKey.Equals( correctlyChangedIds, StringComparison.OrdinalIgnoreCase ) );
		}

		[ Test ]
		[ ExpectedException( typeof( ArgumentException ) ) ]
		public void ProcessNodeService_CanNot_CreateTruthTransition()
		{
			const string sourceIds = "[{\"Flag\":\"false\",\"Id\":\"234kljdf\"},{\"Flag\":\"false\",\"Id\":\"dfgkjfg\"}]";

			var sourceNode = new ProcessNodeDO { ProcessNodeTemplate = new ProcessNodeTemplateDO { TransitionKey = sourceIds } };
			var targetNode = new ProcessNodeDO { Id = 223 };

			this._service.CreateTruthTransition( sourceNode, targetNode );
		}
	}
}