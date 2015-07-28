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
            Account account = LoginDocusign();
            Envelope envelope = CreateAndFillEnvelope(account);
            Assert.IsTrue(envelope.RestError == null);

            IEnvelope envelopeService = new Core.Services.Envelope();
            List<EnvelopeData> envelopeDatas = envelopeService.GetEnvelopeData(envelope);

            Assert.IsNotNull(envelopeDatas);
            //Assert.IsTrue(envelopeDatas.Count > 0); //Todo orkan: remove back when you completed the EnvelopeService.
        }

        #region private methods.
        /// <summary>
        /// Programmatically create an Envelope in DocuSign in the developer sandbox account.
        /// ( Please watch your firewall. It's actualy going to demo docusign server. )
        /// </summary>
        /// <returns>Logged account object ( Docusign.Integrations.Client.Account ).</returns>
        private static Account LoginDocusign()
        {
            // configure application's integrator key and webservice url
            RestSettings.Instance.IntegratorKey = "TEST-34d0ac9c-89e7-4acc-bc1d-24d6cfb867f2";
            RestSettings.Instance.DocuSignAddress = "http://demo.docusign.net";
            RestSettings.Instance.WebServiceUrl = RestSettings.Instance.DocuSignAddress + "/restapi/v2";

            // credentials for sending account
            Account account = new Account
                              {
                                  Email = "hello@orkan.com",
                                  Password = "q.12345R",
                                  AccountIdGuid = Guid.Parse("06e1493c-75be-428a-806e-7480ccff823a"),
                                  AccountId = "1124624",
                                  UserName = "ORKAN ARIKAN"
                              };

            // make the Login API call
            bool result = account.Login();

            Assert.IsTrue(result, "We login to docusign. If this is false, check your credential info and integrator key.");

            return account;
        }

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