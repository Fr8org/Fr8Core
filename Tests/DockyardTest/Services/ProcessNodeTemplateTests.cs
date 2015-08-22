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
	[Category("ProcessNodeTemplateService")]
	public class ProcessNodeTemplateTests : BaseTest
	{
		private IProcessNodeTemplate _processNodeTemplateService;

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
            _processNodeTemplateService = ObjectFactory.GetInstance<IProcessNodeTemplate>();
		}

		[Test]
		public void ProcessNodeTemplateService_Create()
		{
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var sampleNodeTemplate = FixtureData.TestProcessNodeTemplateDO();
                // Create
                _processNodeTemplateService.Create(uow, sampleNodeTemplate);
                //will throw exception if it fails
            }
		}

        [Test]
        public void ProcessNodeTemplateService_Update()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var sampleNodeTemplate = FixtureData.TestProcessNodeTemplateDO();
                // Create
                _processNodeTemplateService.Create(uow, sampleNodeTemplate);
                //will throw exception if it fails

                sampleNodeTemplate.Name = "UpdateTest";

                // Update
                _processNodeTemplateService.Update(uow, sampleNodeTemplate);
                //will throw exception if it fails
            }
        }

        [Test]
        public void ProcessNodeTemplateService_Delete()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var sampleNodeTemplate = FixtureData.TestProcessNodeTemplateDO();
                // Create
                _processNodeTemplateService.Create(uow, sampleNodeTemplate);
                //will throw exception if it fails

                // Delete
                _processNodeTemplateService.Delete(uow, sampleNodeTemplate.Id);
                //will throw exception if it fails
            }
        }
	}
}