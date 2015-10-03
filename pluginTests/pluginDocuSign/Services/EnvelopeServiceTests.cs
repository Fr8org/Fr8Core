using System.Collections.Generic;
using System.Linq;
using Data.Interfaces;

using DocuSign.Integrations.Client;

using NUnit.Framework;

using Utilities;

using Data.Interfaces.DataTransferObjects;
using pluginTests.pluginDocuSign.Interfaces;
using pluginDocuSign.Services;
using pluginTests.Fixtures;
using pluginDocuSign.Interfaces;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace pluginTests.pluginDocuSign.Services
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
			Account account = docusignApiHelper.LoginDocusign(PluginFixtureData.TestDocuSignAccount1(),
				  FixtureData.TestRestSettings1());

			DocuSignEnvelope envelope = docusignApiHelper.CreateAndFillEnvelope(account,
				 PluginFixtureData.TestEnvelope2(account),
				 FixtureData.TestRealPdfFile1(),
				 FixtureData.TestTabCollection1());

			Assert.IsTrue(envelope.RestError == null,
				 "The CreateAndFillEnvelope request contained at least one invalid parameter.");

			IDocuSignEnvelope envelopeService = new DocuSignEnvelope();
			var env = new DocuSignEnvelope();
			IList<EnvelopeDataDTO> envelopeDatas = envelopeService.GetEnvelopeData(envelope);

			Assert.IsNotNull(envelopeDatas);
			//Assert.IsTrue(envelopeDatas.Count > 0); //Todo orkan: remove back when you completed the EnvelopeService.
		}

		[Test] //use a mock for this instead of actually connecting 
		[Category("Envelope")]
		public void Envelope_Can_Normalize_EnvelopeData_Using_TemplateId()
		{
			RestSettings.Instance.RestTracing = true;

			var envelopeDatas = (new DocuSignEnvelope()).GetEnvelopeDataByTemplate(FixtureData.TestTemplateId).ToList();

			Assert.IsNotNull(envelopeDatas);
		}
	}
}