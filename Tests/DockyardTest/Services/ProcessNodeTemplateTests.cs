using System;
using Core.Interfaces;
using Data.Exceptions;
using Data.Interfaces;
using Data.Entities;
using Data.Migrations;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Services
{
	[TestFixture]
	[Category("ProcessNodeTemplate")]
	public class ProcessNodeTemplateTests : BaseTest
	{
		private IProcessNodeTemplate _processNodeTemplate;

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
            _processNodeTemplate = ObjectFactory.GetInstance<IProcessNodeTemplate>();
		}

		[Test]
		public void ProcessNodeTemplateService_CanCreate()
		{
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var processTemplate = FixtureData.TestProcessTemplate2();
                uow.ProcessTemplateRepository.Add(processTemplate);

                //add processnode to process
                var sampleNodeTemplate = FixtureData.TestProcessNodeTemplateDO();
                sampleNodeTemplate.ParentTemplateId = processTemplate.Id;
                

                // Create
                _processNodeTemplate.Create(uow, sampleNodeTemplate);
                //will throw exception if it fails

                if (uow.ProcessNodeTemplateRepository.GetByKey(sampleNodeTemplate.Id) == null)
                {
                    throw new Exception("Creating logic was passed a null ProcessNodeTemplateDO");
                }

            }
		}

        [Test]
        public void ProcessNodeTemplateService_CanUpdate()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var processTemplate = FixtureData.TestProcessTemplate2();
                uow.ProcessTemplateRepository.Add(processTemplate);

                //add processnode to process
                var sampleNodeTemplate = FixtureData.TestProcessNodeTemplateDO();
                sampleNodeTemplate.ParentTemplateId = processTemplate.Id;
                

                // Create
                _processNodeTemplate.Create(uow, sampleNodeTemplate);
                //will throw exception if it fails

                sampleNodeTemplate.Name = "UpdateTest";

                // Update
                _processNodeTemplate.Update(uow, sampleNodeTemplate);
                //will throw exception if it fails

                if (uow.ProcessNodeTemplateRepository.GetByKey(sampleNodeTemplate.Id).Name != "UpdateTest")
                {
                    throw new Exception("ProcessNodeTemplateDO updating logic was failed.");
                }


            }
        }

        [Test]
        public void ProcessNodeTemplateService_CanDelete()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var processTemplate = FixtureData.TestProcessTemplate2();
                uow.ProcessTemplateRepository.Add(processTemplate);

                //add processnode to process
                var sampleNodeTemplate = FixtureData.TestProcessNodeTemplateDO();
                sampleNodeTemplate.ParentTemplateId = processTemplate.Id;
                
                // Create
                _processNodeTemplate.Create(uow, sampleNodeTemplate);
                //will throw exception if it fails

                // Delete
                _processNodeTemplate.Delete(uow, sampleNodeTemplate.Id);
                //will throw exception if it fails

                if (uow.ProcessNodeTemplateRepository.GetByKey(sampleNodeTemplate.Id) != null)
                {
                    throw new Exception("ProcessNodeTemplateDO deleting logic was failed.");
                }
            }
        }
	}
}