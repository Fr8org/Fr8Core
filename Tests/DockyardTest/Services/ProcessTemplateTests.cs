using Core.Interfaces;
using Data.Entities;
using Data.Exceptions;
using Data.Interfaces;
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
                var curEvent = FixtureData.TestDocuSignEvent1();
                ProcessTemplateDO processTemplate = null;
                _processTemplateService.LaunchProcess(uow, processTemplate, FixtureData.DocuSignEventToCrate(curEvent));
            }
		}

        [Test]
        public void ProcessTemplateService_GetProcessNodeTemplates()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curProcessTemplateDO = FixtureData.TestProcessTemplateWithProcessNodeTemplates();
                uow.ProcessTemplateRepository.Add(curProcessTemplateDO);
                uow.SaveChanges();

                var curProcessNodeTemplates = _processTemplateService.GetProcessNodeTemplates(curProcessTemplateDO);

                Assert.IsNotNull(curProcessNodeTemplates);
                Assert.AreEqual(curProcessTemplateDO.ProcessNodeTemplates.Count, curProcessNodeTemplates.Count);
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