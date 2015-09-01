using System.Linq;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Entities
{
    [TestFixture]
    public class ProcessTemplateTests : BaseTest
    {
        [Test]
        [Category("ProcessTemplate")]
        public void ProcessTemplate_ShouldBeAssignedStartingProcessNodeTemplate()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var processTemplate = FixtureData.TestProcessTemplate2();
                uow.ProcessTemplateRepository.Add(processTemplate);

                var processNodeTemplate = FixtureData.TestProcessNodeTemplateDO();
                uow.ProcessNodeTemplateRepository.Add(processNodeTemplate);
                processTemplate.StartingProcessNodeTemplate = processNodeTemplate;

                uow.SaveChanges();

                var result = uow.ProcessTemplateRepository.GetQuery()
                    .SingleOrDefault(pt => pt.StartingProcessNodeTemplateId == processNodeTemplate.Id);

                Assert.AreEqual(processNodeTemplate.Id, result.StartingProcessNodeTemplate.Id);
                Assert.AreEqual(processNodeTemplate.Name, result.StartingProcessNodeTemplate.Name);


            }
        }

     
    }
}