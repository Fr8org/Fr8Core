using Core.Interfaces;
using Data.Exceptions;
using Data.Interfaces;
using Data.Migrations;
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
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var envelope = FixtureData.TestEnvelope1();
                Data.Entities.ProcessTemplateDO processTemplate = null;
                _processTemplateService.LaunchProcess(uow, processTemplate, envelope);
            }
		}
	}
}