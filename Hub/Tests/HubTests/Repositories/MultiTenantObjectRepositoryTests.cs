using System.Collections.Generic;
using System.Linq;
using Data.Interfaces;
using Fr8.Infrastructure.Data.Manifests;
using Hub.StructureMap;
using NUnit.Framework;
using StructureMap;
using Fr8.Testing.Unit;

namespace HubTests.Repositories
{
    [TestFixture]
    [Category("MultiTenantObjectRepository")]
    public class MultiTenantObjectRepositoryTests : BaseTest
    {
        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                foreach (var obj in GenerateTestData())
                {
                    uow.MultiTenantObjectRepository.Add(obj, "account");    
                }

                uow.SaveChanges();
            }
        }

        private static List<DocuSignEnvelopeCM> GenerateTestData()
        {
            List<DocuSignEnvelopeCM> objects = new List<DocuSignEnvelopeCM>();

            for (int i = 0; i < 30; i ++)
            {
                objects.Add(new DocuSignEnvelopeCM
                {
                    EnvelopeId = "id" + i,
                    Status = "status " + (i%3)
                });
            }

            return objects;
        }

        private static void CheckResults(DocuSignEnvelopeCM[] expected, DocuSignEnvelopeCM[] actual)
        {
            Assert.AreEqual(expected.Length, actual.Length);
            foreach (var obj in expected)
            {
                bool found = false;
                
                foreach (var actualObj in actual)
                {
                    if (obj.EnvelopeId == actualObj.EnvelopeId && obj.EnvelopeId == actualObj.EnvelopeId)
                    {
                        found = true;
                        break;
                    }
                }

                Assert.IsTrue(found);
            }
        }

        [Test]
        public void CanQueryAll()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var expected = GenerateTestData().ToArray();
                var actual = uow.MultiTenantObjectRepository.AsQueryable<DocuSignEnvelopeCM>("account").ToArray();
                CheckResults(expected, actual);
            }
        }

        [Test]
        public void CanQueryWithOneWhere()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var expected = GenerateTestData().Where(x => x.EnvelopeId == "id3").ToArray();
                var actual = uow.MultiTenantObjectRepository.AsQueryable<DocuSignEnvelopeCM>("account").Where(x=>x.EnvelopeId == "id3").ToArray();
                CheckResults(expected, actual);
            }
        }

        [Test]
        public void CanQueryWithMultipleWhere()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var expected = GenerateTestData().Where(x => x.EnvelopeId == "id3" && x.EnvelopeId.Contains("id")).ToArray();
                var actual = uow.MultiTenantObjectRepository.AsQueryable<DocuSignEnvelopeCM>("account")
                    .Where(x => x.EnvelopeId == "id3")
                    .Where(x => x.EnvelopeId.Contains("id"))
                    .ToArray();
                CheckResults(expected, actual);
            }
        }

        [Test]
        public void CanQueryWithWhereWithMultipleProperties()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var expected = GenerateTestData().Where(x => x.EnvelopeId == "id3" && x.Status == "status0").ToArray();
                var actual = uow.MultiTenantObjectRepository.AsQueryable<DocuSignEnvelopeCM>("account").Where(x => x.EnvelopeId == "id3" && x.Status == "status0").ToArray();
                CheckResults(expected, actual);
            }
        }
    }
}
