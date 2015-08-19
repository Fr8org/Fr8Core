using System.Collections.Generic;
using System.Linq;
using Data.Interfaces;

using DocuSign.Integrations.Client;

using NUnit.Framework;

using Utilities;

using UtilitiesTesting;
using UtilitiesTesting.DocusignTools;
using UtilitiesTesting.DocusignTools.Interfaces;
using UtilitiesTesting.Fixtures;
using Data.Interfaces.DataTransferObjects;

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
            Account account = docusignApiHelper.LoginDocusign(FixtureData.TestAccount1(),
                                                              FixtureData.TestRestSettings1());

            Envelope envelope = docusignApiHelper.CreateAndFillEnvelope(account,
                                                                        FixtureData.TestEnvelope2(account),
                                                                        FixtureData.TestRealPdfFile1(),
                                                                        FixtureData.TestTabCollection1());

            Assert.IsTrue(envelope.RestError == null, "The CreateAndFillEnvelope request contained at least one invalid parameter.");

            IEnvelope envelopeService = new Data.Wrappers.DocuSignEnvelope();
            List<EnvelopeDataDTO> envelopeDatas = envelopeService.GetEnvelopeData(envelope);

            Assert.IsNotNull(envelopeDatas);
            //Assert.IsTrue(envelopeDatas.Count > 0); //Todo orkan: remove back when you completed the EnvelopeService.
        }

        [Test]
        [Category("Envelope")]
        public void Envelope_Can_Normalize_EnvelopeData_Using_TemplateId()
        {

            RestSettings.Instance.RestTracing = true;

            var envelopeDatas = (new Data.Wrappers.DocuSignEnvelope())
                                .GetEnvelopeData(FixtureData.TestTeamplateId).ToList();

            Assert.IsNotNull(envelopeDatas);
        }

    }
}