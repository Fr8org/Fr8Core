using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Core.Services;

using Data.Entities;
using Data.Interfaces;

using DocuSign.Integrations.Client;

using NUnit.Framework;

using StructureMap;
using UtilitiesTesting;

using Utilities;

using UtilitiesTesting.DocusignTools;
using UtilitiesTesting.Fixtures;

using Account = DocuSign.Integrations.Client.Account;
using Envelope = DocuSign.Integrations.Client.Envelope;

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
            Account account = DocusignApi.LoginDocusign(DocusignAccount.GetStubAccount());

            Envelope envelope = CreateAndFillEnvelope(account);
            Assert.IsTrue(envelope.RestError == null);

            IEnvelope envelopeService = new Core.Services.Envelope();
            List<EnvelopeData> envelopeDatas = envelopeService.GetEnvelopeData(envelope);

            Assert.IsNotNull(envelopeDatas);
            //Assert.IsTrue(envelopeDatas.Count > 0); //Todo orkan: remove back when you completed the EnvelopeService.
        }

        #region private methods.


        /// <summary>
        /// Create envelope with current account info and fill envelope with some gibberish data, and return it back.
        /// </summary>
        /// <param name="account">Docusign Account that includes login info.</param>
        /// <returns>Envelope of Docusign.</returns>
        private static Envelope CreateAndFillEnvelope(Account account)
        {
            // create envelope object and assign login info
            Envelope envelope = new Envelope
                                {
                                    // assign account info from above
                                    Login = account,
                                    // "sent" to send immediately, "created" to save envelope as draft
                                    Status = "created",
                                    Created = DateTime.UtcNow
                                };

            string fullPathToExampleDocument = Path.Combine(Environment.CurrentDirectory, "App_Data", "docusign_examplephoto.png");

            // create a new DocuSign envelope...
            envelope.Create(fullPathToExampleDocument);

            List<TextTab> textTabs = new List<TextTab>
                                     {
                                         new TextTab
                                         {
                                             required = false,
                                             height = 200,
                                             width = 200,
                                             xPosition = 200,
                                             yPosition = 200,
                                             name = "Amount",
                                             value = "40"
                                         }
                                     };

            //populate it with some Tabs with values. Example "Amount" is a text field with value "45".
            envelope.AddTabs(new TabCollection
                             {
                                 textTabs = textTabs
                             });

            return envelope;
        }
        #endregion

    }
}