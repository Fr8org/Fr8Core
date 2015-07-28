using System;
using Core.Interfaces;
using Data.Entities;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;

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
		public void ProcessNodeService_Can_CreateTruthTransition()
		{
			const string sourceIds = "[\"false\":\"234kljdf\", \"false\":\"dfgkjfg\", \"false\":\"xcvbvc\"]";
			const string targetIds = "[\"true\":\"234kljdf\", \"false\":\"dfgkjfg\", \"true\":\"dfgkjfg\"]";
			const string correctlyChangedIds = "[\"true\":\"234kljdf\", \"true\":\"dfgkjfg\", \"false\":\"xcvbvc\"]";

			var sourceNode = new ProcessNodeDO { ProcessNodeTemplate = new ProcessNodeTemplateDO { TransitionKey = sourceIds } };
			var targetNode = new ProcessNodeDO { ProcessNodeTemplate = new ProcessNodeTemplateDO { TransitionKey = targetIds } };

			this._service.CreateTruthTransition( sourceNode, targetNode );

			Assert.IsTrue( sourceNode.ProcessNodeTemplate.TransitionKey.Equals( correctlyChangedIds, StringComparison.OrdinalIgnoreCase ) );
		}
	}
}