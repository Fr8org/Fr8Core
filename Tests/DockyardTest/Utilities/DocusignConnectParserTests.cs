using System;
using System.IO;
using NUnit.Framework;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Utilities
{
	[TestFixture]
	[Category("DocusignConnectParser")]
	public class DocusignConnectParserTests : BaseTest
	{
		private string _xmlPayloadFullPath;

		[SetUp]
		public override void SetUp()
		{
			_xmlPayloadFullPath = FixtureData.FindXmlPayloadFullPath(Environment.CurrentDirectory);
			if (_xmlPayloadFullPath == string.Empty)
				throw new Exception("XML payload file for testing DocuSign notification is not found.");
		}

		[Test]
		public void DocusignConnectParser_CanParseEnvelopeData()
		{
			var xml = File.ReadAllText(_xmlPayloadFullPath);
			var info = FixtureData.TestDocuSignEnvelopeInformation();
			var docuSignEnvelopeInformation = DocuSignConnectParser.GetEnvelopeInformation(xml);

			Assert.NotNull(docuSignEnvelopeInformation);
			Assert.AreEqual(docuSignEnvelopeInformation.EnvelopeStatus.EnvelopeId, info.EnvelopeStatus.EnvelopeId);
		}
	}
}