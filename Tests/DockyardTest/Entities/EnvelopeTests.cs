using System.Collections.Generic;
using System.Linq;

using Core.Services;

using Data.Entities;
using Data.Interfaces;

using DocusignApiWrapper;
using DocusignApiWrapper.Interfaces;

using NUnit.Framework;

using StructureMap;

using Utilities;

using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

using Account = DocuSign.Integrations.Client.Account;
using Envelope = DocuSign.Integrations.Client.Envelope;

namespace DockyardTest.Entities
{
    [TestFixture]
    public class EnvelopeTests : BaseTest
    {
        private readonly IDocusignApiHelper docusignApiHelper;

        public EnvelopeTests()
        {
            docusignApiHelper = new DocusignApiHelper();
        }

        [Test]
        [Category("Envelope")]
        public void Envelope_Change_Status()
        {
            const EnvelopeDO.EnvelopeState newStatus = EnvelopeDO.EnvelopeState.Created;
            const EnvelopeDO.EnvelopeState updatedStatus = EnvelopeDO.EnvelopeState.Delivered;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.EnvelopeRepository.Add(new EnvelopeDO { Id = 1, Status = newStatus, DocusignEnvelopeId = "23" });
                uow.SaveChanges();

                var createdEnvelope = uow.EnvelopeRepository.GetQuery().FirstOrDefault();
                Assert.NotNull(createdEnvelope);
                Assert.AreEqual(newStatus, createdEnvelope.Status);

                createdEnvelope.Status = updatedStatus;
                uow.SaveChanges();

                var updatedEnvelope = uow.EnvelopeRepository.GetQuery().FirstOrDefault();
                Assert.NotNull(updatedEnvelope);
                Assert.AreEqual(updatedStatus, updatedEnvelope.Status);
            }
        }

        [Test]
        [Category("Envelope")]
        public void Envelope_Can_Normalize_EnvelopeData()
        {
            Account account = docusignApiHelper.LoginDocusign(DocusignAccount.GetStubAccount());
            Envelope envelope = docusignApiHelper.CreateAndFillEnvelope(account,
                                                                        FixtureData.CreateEnvelope(account),
                                                                        FixtureData.FullFilePathToDocument(),
                                                                        FixtureData.GetTabCollection());

            Assert.IsTrue(envelope.RestError == null);

            IEnvelope envelopeService = new Core.Services.Envelope();
            List<EnvelopeData> envelopeDatas = envelopeService.GetEnvelopeData(envelope);

            Assert.IsNotNull(envelopeDatas);
            //Assert.IsTrue(envelopeDatas.Count > 0); //Todo orkan: remove back when you completed the EnvelopeService.
        }

    }
}