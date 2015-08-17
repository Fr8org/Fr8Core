using System.Linq;

using Data.Entities;
using Data.Interfaces;

using NUnit.Framework;

using StructureMap;

using UtilitiesTesting;

namespace DockyardTest.Entities
{
    [TestFixture]
    public class EnvelopeTests : BaseTest
    {
        [Test]
        [Category("Envelope")]
        public void Envelope_Change_Status()
        {
            const EnvelopeDO.EnvelopeState newStatus = EnvelopeDO.EnvelopeState.Created;
            const EnvelopeDO.EnvelopeState updatedStatus = EnvelopeDO.EnvelopeState.Delivered;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.EnvelopeRepository.Add(new EnvelopeDO { Id = 1, EnvelopeStatus = newStatus, DocusignEnvelopeId = "23" });
                uow.SaveChanges();

                var createdEnvelope = uow.EnvelopeRepository.GetQuery().FirstOrDefault();
                Assert.NotNull(createdEnvelope);
                Assert.AreEqual(newStatus, createdEnvelope.Status);

                createdEnvelope.EnvelopeStatus = updatedStatus;
                uow.SaveChanges();

                var updatedEnvelope = uow.EnvelopeRepository.GetQuery().FirstOrDefault();
                Assert.NotNull(updatedEnvelope);
                Assert.AreEqual(updatedStatus, updatedEnvelope.Status);
            }
        }
    }
}