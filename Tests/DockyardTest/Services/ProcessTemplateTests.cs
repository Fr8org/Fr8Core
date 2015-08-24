using Core.Interfaces;
using Data.Entities;
using Data.Exceptions;
using Data.Interfaces;
using Data.Migrations;
using NUnit.Framework;
using StructureMap;
using System.Collections.Generic;
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
                var curEvent = FixtureData.TestDocuSignEvent1();
                Data.Entities.ProcessTemplateDO processTemplate = null;
                _processTemplateService.LaunchProcess(uow, processTemplate, curEvent);
            }
		}

		//[Test]
  //      public void TemplateRegistrationCollections_ShouldMakeIdentical()
  //      {
  //          var curSubscriptions = FixtureData.DocuSignTemplateSubscriptionList1();
  //          var newSubscriptions = FixtureData.DocuSignTemplateSubscriptionList2();
  //          using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
  //          {
  //              _processTemplateService.MakeCollectionEqual(uow, curSubscriptions, newSubscriptions);
  //          }
  //          CollectionAssert.AreEquivalent(newSubscriptions, curSubscriptions);
  //      }
    }
}