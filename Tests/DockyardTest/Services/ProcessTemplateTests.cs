using Core.Interfaces;
using Data.Exceptions;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Services
{
	[TestFixture]
	[Category("ProcessTemplateService")]
	public class ProcessTemplateTests : BaseTest
	{
		private IProcessTemplate _processTemplateService;

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
			_processTemplateService = ObjectFactory.GetInstance<IProcessTemplate>();
		}

		[Test]
		[ExpectedException(typeof (EntityNotFoundException))]
		public void ProcessTemplateService_CanNot_LaunchProcess()
		{
			var envelope = FixtureData.TestEnvelope1();
			_processTemplateService.LaunchProcess(2, envelope);
		}
	}
}