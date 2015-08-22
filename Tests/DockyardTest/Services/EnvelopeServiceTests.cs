using System.Collections.Generic;

using Data.Interfaces;

using DocuSign.Integrations.Client;

using NUnit.Framework;

using Utilities;

using UtilitiesTesting;
using UtilitiesTesting.DocusignTools;
using UtilitiesTesting.DocusignTools.Interfaces;
using UtilitiesTesting.Fixtures;
using Data.Interfaces.DataTransferObjects;
using Data.Wrappers;

namespace DockyardTest.Services
{
    [TestFixture]
    public class EnvelopeServiceTests : BaseTest
    {
        private readonly IDocusignApiHelper docusignApiHelper;

        public EnvelopeServiceTests()
        {
            docusignApiHelper = new DocusignApiHelper();
        }

        [Test]
        [Category("Envelope")]
        public void Envelope_Can_Normalize_EnvelopeData()
        {
            Account account = docusignApiHelper.LoginDocusign(FixtureData.TestDocuSignAccount1(),
                                                              FixtureData.TestRestSettings1());

            DocuSignEnvelope envelope = docusignApiHelper.CreateAndFillEnvelope(account,
                                                                        FixtureData.TestEnvelope2(account),
                                                                        FixtureData.TestRealPdfFile1(),
                                                                        FixtureData.TestTabCollection1());

            Assert.IsTrue(envelope.RestError == null, "The CreateAndFillEnvelope request contained at least one invalid parameter.");

            IEnvelope envelopeService = new Data.Wrappers.DocuSignEnvelope();
            var env = new Data.Wrappers.DocuSignEnvelope();
            List <EnvelopeDataDTO> envelopeDatas = envelopeService.GetEnvelopeData(envelope);

            Assert.IsNotNull(envelopeDatas);
            //Assert.IsTrue(envelopeDatas.Count > 0); //Todo orkan: remove back when you completed the EnvelopeService.
        }

    }
}