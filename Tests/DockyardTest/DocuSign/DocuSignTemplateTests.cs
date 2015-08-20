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

namespace DockyardTest.DocuSign
{
    [TestFixture]
    public class DocuSignTemplateTests : BaseTest
    {
        private readonly IDocusignApiHelper docusignApiHelper;

        public DocuSignTemplateTests()
        {
            docusignApiHelper = new DocusignApiHelper();
        }

        [Test]
        [Category("DocuSignIntegration")]
        public void Template_Can_Get_Template()
        {
            DocuSignAccount account = docusignApiHelper.LoginDocusign(FixtureData.TestDocuSignAccount1(),
                                                              FixtureData.TestRestSettings1());

            TemplateInfo curTemplate = FixtureData.TestDocuSignTemplateInfo1();
            


            Assert.IsNotNull(account); //Todo orkan: remove back when you completed the EnvelopeService.
        }

    }
}