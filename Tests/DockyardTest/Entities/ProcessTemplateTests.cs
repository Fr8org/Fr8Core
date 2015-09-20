using System.Linq;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Core.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;

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

                var processNodeTemplate = FixtureData.TestProcessNodeTemplateDO2();
                uow.ProcessNodeTemplateRepository.Add(processNodeTemplate);
                processTemplate.StartingProcessNodeTemplate = processNodeTemplate;

                uow.SaveChanges();

                var result = uow.ProcessTemplateRepository.GetQuery()
                    .SingleOrDefault(pt => pt.StartingProcessNodeTemplateId == processNodeTemplate.Id);

                Assert.AreEqual(processNodeTemplate.Id, result.StartingProcessNodeTemplate.Id);
                Assert.AreEqual(processNodeTemplate.Name, result.StartingProcessNodeTemplate.Name);
            }
        }

        [Test]
        [Category("ProcessTemplate")]
        public void GetStandardEventSubscribers_ReturnsProcessTemplates()
        {
            FixtureData.TestProcessTemplateWithSubscribeEvent();
            IProcessTemplate curProcessTemplate = ObjectFactory.GetInstance<IProcessTemplate>();
            EventReportMS curEventReport = FixtureData.StandardEventReportFormat();

            var result = curProcessTemplate.GetMatchingProcessTemplates("testuser1", curEventReport);

            Assert.IsNotNull(result);
            Assert.Greater(result.Count, 0);
            Assert.Greater(result.Where(name => name.Name.Contains("StandardEventTesting")).Count(), 0);
        }

        [Test]
        [Category("ProcessTemplate")]
        [ExpectedException(ExpectedException = typeof(System.ArgumentNullException))]
        public void GetStandardEventSubscribers_UserIDEmpty_ThrowsException()
        {
            IProcessTemplate curProcessTemplate = ObjectFactory.GetInstance<IProcessTemplate>();

            curProcessTemplate.GetMatchingProcessTemplates("", new EventReportMS());
        }

        [Test]
        [Category("ProcessTemplate")]
        [ExpectedException(ExpectedException = typeof(System.ArgumentNullException))]
        public void GetStandardEventSubscribers_EventReportMSNULL_ThrowsException()
        {
            IProcessTemplate curProcessTemplate = ObjectFactory.GetInstance<IProcessTemplate>();

            curProcessTemplate.GetMatchingProcessTemplates("UserTest", null);
        }
    }
}