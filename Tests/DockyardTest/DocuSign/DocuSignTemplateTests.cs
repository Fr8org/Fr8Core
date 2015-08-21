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
using Data.Migrations;
using Data.Wrappers;
using StructureMap;

namespace DockyardTest.DocuSign
{
    [TestFixture]
    public class DocuSignTemplateTests : BaseTest
    {
        private readonly IDocusignApiHelper docusignApiHelper;
        private readonly IDocuSignTemplate _docusignTemplate;

        public DocuSignTemplateTests()
        {
            docusignApiHelper = new DocusignApiHelper();
            _docusignTemplate = ObjectFactory.GetInstance<IDocuSignTemplate>();
        }

        [Test, Ignore]
        [Category("DocuSignIntegration")]
        public void Template_Can_Get_Template()
        {
            DocuSignAccount account = docusignApiHelper.LoginDocusign(FixtureData.TestDocuSignAccount1(),
                                                              FixtureData.TestRestSettings1());

            TemplateInfo curTemplateInfo = FixtureData.TestDocuSignTemplateInfo1();         
            //post the template to the test account
            TemplateInfo createdTemplateInfo = _docusignTemplate.Create(curTemplateInfo);


            //get it
            //TemplateInfo retrievedTemplateInfo = _docusignTemplate.GetTemplate(createdTemplateInfo.Id);

            //verify
            //delete it



            Assert.IsNotNull(account); //Todo orkan: remove back when you completed the EnvelopeService.
        }

    }
}