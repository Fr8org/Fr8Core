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

        [Test,Ignore]
        public void CanActivateProcessTemplate()
        {
            var curProcessTemplateDO = FixtureData.TestProcessTemplate3();
           string result = _processTemplateService.Activate(curProcessTemplateDO);
           Assert.AreEqual(result, "success");
        }

        [Test, Ignore]
        public void FailsActivateProcessTemplate()
        {
            var curProcessTemplateDO = FixtureData.TestProcessTemplate3();
            string result = _processTemplateService.Activate(curProcessTemplateDO);
            Assert.AreEqual(result, "failed");
        }

        [Test, Ignore]
        public void CanDeactivateProcessTemplate()
        {
            var curProcessTemplateDO = FixtureData.TestProcessTemplate3();
            string result = _processTemplateService.Activate(curProcessTemplateDO);
            Assert.AreEqual(result, "success");
        }

        [Test, Ignore]
        public void FailsDeactivateProcessTemplate()
        {
            var curProcessTemplateDO = FixtureData.TestProcessTemplate3();
            string result = _processTemplateService.Activate(curProcessTemplateDO);
            Assert.AreEqual(result, "failed");
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