using System;
using Core.Interfaces;
using Data.Interfaces;
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
                var sampleNodeTemplate = FixtureData.TestProcessNodeTemplateDO2();
                sampleNodeTemplate.ParentActivityId = processTemplate.Id;

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
                var sampleNodeTemplate = FixtureData.TestProcessNodeTemplateDO2();
                sampleNodeTemplate.ParentActivityId = processTemplate.Id;


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

        // MockDB has boken logic when working with collections of objects of derived types
        // We add object to ProcessNodeTemplateRepository but Delete logic recusively traverse Activity repository.
        [Ignore("MockDB behavior is incorrect")]
        [Test]
        public void ProcessNodeTemplateService_CanDelete()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var processTemplate = FixtureData.TestProcessTemplate2();
                uow.ProcessTemplateRepository.Add(processTemplate);

                //add processnode to process
                var sampleNodeTemplate = FixtureData.TestProcessNodeTemplateDO2();
                sampleNodeTemplate.ParentActivityId = processTemplate.Id;

                // Create
                _processNodeTemplate.Create(uow, sampleNodeTemplate);
                //will throw exception if it fails

                if (uow.ActivityRepository.GetByKey(sampleNodeTemplate.Id) == null)
                {
                    throw new Exception("ProcessNodeTemplateDO add logic was failed.");
                }

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